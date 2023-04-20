using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tgs.StationLetter.ApplicationServices;
public record SendStationLetterCommand(
	string Sender,
	IEnumerable<string> Receivers,
	string Subject,
	string Content);
