using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NinjaTurtles.Tests.Turtles.BranchConditionTurtleTestSuite
{
    public class BranchConditionClassUnderTest
    {
        public int StupidParse(string input)
        {
            if (input == "Seven")
            {
                return 7;
            }
            return -1;
        }

        public int WorkingStupidParse(string input)
        {
            if (input == "Seven")
            {
                return 7;
            }
            return -1;
        }
    }
}
