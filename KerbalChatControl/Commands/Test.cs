using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChatController.Commands
{
    class TestCommand : ICommand
    {
        public void Execute(string[] args)
        {
            ModHandler.ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Stage, true);
        }

        public string GetName()
        {
            return "check";
        }
    }
}
