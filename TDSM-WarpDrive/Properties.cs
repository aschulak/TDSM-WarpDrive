using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WarpDrive
{
	public class Properties : PropertiesFile
	{
		public Properties(String propertiesPath) : base(propertiesPath) { }
		
		public void pushData()
		{			
			setRequiresOp(requiresOp());
		}

		public bool requiresOp()
		{
			string requiresOp = base.getValue("requiresOp");
			if (requiresOp == null || requiresOp.Trim().Length < 0) {
				return true;
			} else {
				return Boolean.Parse(requiresOp);
			}
		}

		public void setRequiresOp(bool OpRequired)
		{
			base.setValue("requiresOp", OpRequired.ToString());				
		}
	}
}
