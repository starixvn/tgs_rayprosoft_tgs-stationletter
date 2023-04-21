namespace Tgs.StationLetter.MessageQueue;

public static class ProcessorTypes
{
	public static readonly Type SendLetterHandlerType = typeof(SendLetterHandler);
	public static readonly Type InboxLettersHandlerType = typeof(InboxLettersHandler);
}
