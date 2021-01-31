﻿using System.Collections.Generic;

namespace Exodrifter.Rumor.Engine
{
	public class AppendDialogNode : DialogNode
	{
		public AppendDialogNode(string speaker, string dialog)
			: base(speaker, dialog) { }

		public AppendDialogNode(string speaker, Expression dialog)
			: base(speaker, dialog) { }

		public override Yield Execute(Rumor rumor)
		{
			var dialog = Dialog.Evaluate(rumor.Scope).AsString().Value;
			rumor.State.AppendDialog(Speaker, dialog);
			return new ForAdvance();
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as AppendDialogNode);
		}

		public bool Equals(AppendDialogNode other)
		{
			if (other == null)
			{
				return false;
			}

			return Speaker == other.Speaker
				&& Dialog == other.Dialog;
		}

		public override int GetHashCode()
		{
			return Util.GetHashCode(Speaker, Dialog);
		}

		public override string ToString()
		{
			return Speaker + "+ " + Dialog;
		}
	}
}
