using System.Text.Json.Serialization;
using FluentdForward.OpenTelemetry.MessagePack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Tgs.StationLetterService;
using Tgs.StationLetterService.Client;
using Tgs.StationLetterService.MessageQueue;
using Tgs.StationLetterService.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Logging
	.AddOpenTelemetry(options =>
	{
		options.IncludeFormattedMessage = true;
		options.ParseStateValues = true;
		_ = options.AddFluentdForwardExporter(fluentOptions =>
		{
			var nodeIp = builder.Configuration.GetValue<string>("LogServiceDomain")!;
			fluentOptions.Host = nodeIp;
			fluentOptions.Tag = builder.Configuration.GetValue<string>("OTEL_SERVICE_NAME")!;
			fluentOptions.UseMessagePack(LogRecordFormatterResolver.GetResolverInstanceWithExtendInfo(
				"MachineInfo",
				() => new
				{
					Environment.MachineName
				}));
		});
	});

builder.Services
	.AddRouting(routeOpt => routeOpt.LowercaseUrls = true)
	.AddControllers()
	.AddMvcOptions(mvcOptions => mvcOptions.AddWebApiToolkitFilters())
	.AddJsonOptions(jsonOpt => jsonOpt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services
	.AddSingleton(sp => new RedisConnectionManager(
		builder.Configuration.GetConnectionString("Redis")!))
	.AddStationLetter()
	.AddStationLetterRepositories(
		dbConfig => dbConfig
			.UseNpgsql("name=Tgs"),
		sp => sp.GetRequiredService<RedisConnectionManager>().Database)
	.AddBrandServiceClient((brandOptions, _) => brandOptions.BrandCertificateBaseUrl =
		builder.Configuration.GetValue<Uri>("BrandAllPlayers:BrandServiceBaseUrl")!)
	.AddPlayerServiceClient((playerOptions, _) => playerOptions.BaseUrl =
		builder.Configuration.GetValue<Uri>("BrandAllPlayers:PlayerServiceBaseUrl")!)
	.AddTransient<IStationLetterClient, InternalStationLetterClient>()
	.AddMessageQueue()
	.AddNatsMessageQueue(natsConfig => natsConfig
		.ConfigQueueOptions((options, _) => options.Url =
			builder.Configuration.GetConnectionString("Nats")!)
		.AddHandler(
			ProcessorTypes.SendLetterHandlerType,
			"tgs.station-letter.delivery",
			"tgs.station-letter.delivery")
		.AddHandler(
			ProcessorTypes.InboxLettersHandlerType,
			"tgs.station-letter.inbox",
			"tgs.station-letter.inbox")
		.AddHandler(
			ProcessorTypes.DispatchReceiversLetterHandlerType,
			"tgs.station-letter.dispatch.receivers",
			"tgs.station-letter.dispatch.receivers")
		.AddHandler(
			Tgs.StationLetter.BrandAllPlayers.MessageQueue.ProcessorTypes.DispatchBrandAllPlayersLetterHandlerType,
			"tgs.station-letter.dispatch.brandallplayers",
			"tgs.station-letter.dispatch.brandallplayers"))
	.AddNatsGlobPatternExchange("*");

builder.Services
	.AddSwaggerGen(swaggerOptions =>
	{
		swaggerOptions.ApplyWebApiToolkitSupport();

		var xmlFileNames = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.xml");
		foreach (var xmlFileName in xmlFileNames)
			swaggerOptions.IncludeXmlComments(xmlFileName);
	});

builder.Services
	.AddHealthChecks()
	.Services
	.AddOpenTelemetry()
	.WithTracing(traceBuilder => traceBuilder
		.SetResourceBuilder(ResourceBuilder.CreateDefault()
			.AddEnvironmentVariableDetector())
		.AddSource("Tgs.*")
		.AddAspNetCoreInstrumentation()
		.AddOtlpExporter())
	.WithMetrics(meterBuiler => meterBuiler
		.AddRuntimeInstrumentation()
		.AddAspNetCoreInstrumentation()
		.AddOtlpExporter())
	.Services
	.Configure<AspNetCoreInstrumentationOptions>(options => options.Filter =
		httpContext => !httpContext.Request.Path.HasValue
			|| !httpContext.Request.Path.StartsWithSegments("/swagger")
			&& !httpContext.Request.Path.StartsWithSegments("/healthz")
			&& !httpContext.Request.Path.StartsWithSegments("/metrics")
			&& !new[] { "html", "ico", "png", "jpg" }.Any(
				ext => httpContext.Request.Path.Value.EndsWith($".{ext}")));

var app = builder.Build();

app.MapControllers();
app.MapSwagger();
app.UseSwaggerUI();
app.MapHealthChecks("/healthz");

app.Run();
