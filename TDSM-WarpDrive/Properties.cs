using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Envoy.TDSM_WarpDrive
{
  	
	public class Properties : PropertiesFile
    {
		
		
        public Properties(String propertiesPath) : base(propertiesPath)
        {
        }
     
        public void pushData()
        {
            setGlobalOwnershipEnforced(globalOwnershipEnforced());
            setRequiresOp(requiresOp());
        }

        public bool globalOwnershipEnforced()
        {
            string globalOwnershipEnforced = base.getValue("globalOwnershipEnforced");
            if (globalOwnershipEnforced == null || globalOwnershipEnforced.Trim().Length < 0) {
                return true;
            } else {
                return Boolean.Parse(globalOwnershipEnforced);
            }
        }

        public void setGlobalOwnershipEnforced(bool globalOwnershipEnforced)
        {
            base.setValue("globalOwnershipEnforced", globalOwnershipEnforced.ToString());
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