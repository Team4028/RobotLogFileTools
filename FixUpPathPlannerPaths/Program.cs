using System;
using System.Collections.Generic;
using FileHelpers;

using FixUpPathPlannerPaths.Entities;

namespace FixUpPathPlannerPaths
{
    class Program
    {

        const string PATH_NAME = @"LeftTurn_v3";
        const string FOLDER_PATH = @"C:\Users\xtobr\Source\Repos\FRC4028\2019_PathFollowing\src\main\deploy\paths\output";

        static void Main(string[] args)
        {
            // left side
            ProcessFile(PATH_NAME, @"left", @"right");

            // right side
            ProcessFile(PATH_NAME, @"right", @"left");
        }

        static void ProcessFile(string pathName, string sourceSide, string targetSide)
        {
            // create engine
            var engine = new FileHelperEngine<PathSegmentBE>();

            // 1. process left file:    LeftTurn_v3_left.csv

            // build source filename
            string sourceFileName = $"{PATH_NAME}_{sourceSide}.csv";
            string sourceFilePathName = System.IO.Path.Combine(FOLDER_PATH, sourceFileName);

            // read orginal file
            var segments = engine.ReadFile(sourceFilePathName);

            // loop thru and adjust values
            foreach (var segment in segments)
            {
                segment.x = segment.x * 12.0M;
                segment.y = segment.y * 12.0M;
                segment.position = segment.position * 12.0M;
            }

            // write out file with header  LeftTurn_v3.left.pf1.csv
            string targetFileName = $"{PATH_NAME}.{targetSide}.pf1.csv";
            string targetFilePathName = System.IO.Path.Combine(FOLDER_PATH, targetFileName);

            engine.HeaderText = engine.GetFileHeader();
            engine.WriteFile(targetFilePathName, segments);
        }
    }
}
