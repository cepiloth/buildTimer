// Guids.cs
// MUST match guids.h
using System;

namespace KnowledgeBaseSoftware.SolutionBuildTimer
{
    static class GuidList
    {
        public const string guidSolutionBuildTimerPkgString = "5a5015f3-d597-47b5-8ffb-a02f5775799d";
        public const string guidSolutionBuildTimerCmdSetString = "df8954f8-d0a6-4eb7-bd43-537d54311c41";

        public static readonly Guid guidSolutionBuildTimer_2013CmdSet = new Guid(guidSolutionBuildTimerCmdSetString);
    };
}