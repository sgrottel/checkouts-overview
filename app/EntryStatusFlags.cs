using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SG.Checkouts_Overview
{

	[Flags]
	public enum EntryStatusFlags : ushort
	{
		Unknown = 0,				// not evaluated, yet
		Evaluating = 1 << 0,		// being currently evaluated
		Evaluated = 1 << 1,			// Evaluation completed
		PathUnreachable = 1 << 2,	// path not reachable
		EvaluationFailed = 1 << 3,	// Evaluation failed (e.g. type), see last message
		Modified = 1 << 4,			// Local modifications
		OnBranch = 1 << 5,			// Not on main branch
		Untracked = 1 << 6,			// No tracked branch configured
		Incoming = 1 << 7,			// Incoming changes
		Outgoing = 1 << 8			// outgoing changes
	}

}
