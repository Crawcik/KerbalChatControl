using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChatController.Commands
{
    public interface ICommand
    {
        string GetName();
        void Execute(string[] args);
    }
}
