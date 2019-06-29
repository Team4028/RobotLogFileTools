using System;
using System.Collections.Generic;
using System.Text;

using SpreadsheetGear;
using SpreadsheetGear.Charts;

using ProcessLogFile.Entities;

namespace ProcessLogFile
{
    /// <summary>
    /// This class used a rd party library to build one or more excel graphs
    /// </summary>
    static class GraphBuilder
    {
        public static void ProcessLogFile(string logFilePathName, CfgOptionsBE config)
        {
            // Activate SpreadsheetGear
            SpreadsheetGear.Factory.SetSignedLicense("SpreadsheetGear.License, Type=Trial, Product=BND, Expires=2019-07-27, Company=Tom Bruns, Email=xtobr39@hotmail.com, Signature=orH+RFO9hRUB8SJXBSWQZJuXP9OfSkV9fLcU9suehfgA#dgunwBK9VssTgnfowKGWaqMNfVgwVetxEWbayzGM1uIA#K");

            // Create a new empty workbook in a new workbook set.
            SpreadsheetGear.IWorkbookSet workbookSet = SpreadsheetGear.Factory.GetWorkbookSet();

            // import the csv file
            SpreadsheetGear.IWorkbook workbook = workbookSet.Workbooks.Open(logFilePathName);
            
            // get a reference to the active (only) worksheet
            SpreadsheetGear.IWorksheet dataWorksheet = workbook.ActiveWorksheet;
            dataWorksheet.Name = System.IO.Path.GetFileNameWithoutExtension(logFilePathName);

            // resize column widths to fit header text
            dataWorksheet.UsedRange.Columns.AutoFit();

            // freeze 1st row
            dataWorksheet.WindowInfo.ScrollColumn = 0;
            dataWorksheet.WindowInfo.SplitColumns = 0;
            dataWorksheet.WindowInfo.ScrollRow = 0;
            dataWorksheet.WindowInfo.SplitRows = 1;
            dataWorksheet.WindowInfo.FreezePanes = true;

            // build index of column names
            var columnNameIndex = BuildColumnNameXref(dataWorksheet);

            // build a new graph for each one that was configured
            foreach(GraphBE graph in config.Graphs)
            {
                BuildGraph(dataWorksheet, graph, columnNameIndex);
            }

            // save the workbook
            string xlsFileName = System.IO.Path.ChangeExtension(logFilePathName, @".xlsx");

            workbook.SaveAs(xlsFileName, FileFormat.OpenXMLWorkbook);
        }

        /// <summary>
        /// build a xref of the columns in the log file
        /// </summary>
        /// <param name="dataWorksheet">The data worksheet.</param>
        /// <returns>Dictionary&lt;System.String, System.Int32&gt;.</returns>
        private static Dictionary<string, int> BuildColumnNameXref(SpreadsheetGear.IWorksheet dataWorksheet)
        {
            Dictionary<string, int> colNameXref = new Dictionary<string, int>();

            IRange usedRange = dataWorksheet.UsedRange;

            IRange usedColumns = usedRange.Columns;

            int columnCount = usedColumns.ColumnCount;

            for(int colIndex = 0; colIndex <= columnCount-1; colIndex++)
            {
                colNameXref.Add(dataWorksheet.Cells[0, colIndex].Text, colIndex);
            }

            return colNameXref;
        }

        /// <summary>
        /// Builds the graph.
        /// </summary>
        /// <param name="dataWorksheet">The data worksheet.</param>
        /// <param name="graphConfig">The graph configuration.</param>
        /// <param name="columnNameIndex">Index of the column name.</param>
        /// <exception cref="ApplicationException">... Error building graph: [{graphConfig.Name}], Expected cols: [{errList}</exception>
        private static void BuildGraph(SpreadsheetGear.IWorksheet dataWorksheet, GraphBE graphConfig, Dictionary<string, int> columnNameIndex)
        {
            SpreadsheetGear.IWorkbook workbook = dataWorksheet.Workbook;
            int columnIdx = -1;
            int xAxisTargetColumnIdx = -1;
            string xAxisColumnName = graphConfig.XAxis.FromColumnName;
            List<string> missingColumnNames = new List<string>();

            // step 1: find the column we want to target for the XAxis
            if (!columnNameIndex.TryGetValue(xAxisColumnName, out xAxisTargetColumnIdx))
            {
                missingColumnNames.Add(xAxisColumnName);
            }

            // step 2: find the columns we want to target for the YAxis
            Dictionary<int, string> yAxisTargetColIdxs = new Dictionary<int, string>();
            foreach(string yAxisColumnName in graphConfig.YAxis.FromColumnNames)
            {
                if(columnNameIndex.TryGetValue(yAxisColumnName, out columnIdx))
                {
                    yAxisTargetColIdxs.Add(columnIdx, yAxisColumnName);
                }
                else
                {
                    missingColumnNames.Add(yAxisColumnName);
                }
            }

            // step 3: find the columns we want to reference for the Gains
            string pidGainsColumnName = graphConfig.Gains.PIDGains;
            string followerGainsColumnName = graphConfig.Gains.FollowerGains;

            int pidGainsColumnIdx = -1;
            int followerGainsColumnIdx = -1;

            if (!string.IsNullOrEmpty(pidGainsColumnName))
            {
                if (!columnNameIndex.TryGetValue(pidGainsColumnName, out pidGainsColumnIdx))
                {
                    missingColumnNames.Add(pidGainsColumnName);
                }
            }

            if (!string.IsNullOrEmpty(followerGainsColumnName))
            {
                if (!columnNameIndex.TryGetValue(followerGainsColumnName, out followerGainsColumnIdx))
                {
                    missingColumnNames.Add(followerGainsColumnName);
                }
            }

            // stop if any were missing
            if (missingColumnNames.Count > 0)
            {
                string errList = String.Join(",", missingColumnNames);
                throw new ApplicationException($"... Error building graph: [{graphConfig.Name}], Expected cols: [{errList}] cannot be found!");
            }

            // Step 4: add a new worksheet to hold the chart
            IWorksheet chartSheet = workbook.Worksheets.Add();
            chartSheet.Name = graphConfig.Name;

            // Step 5.1: time to build the chart
            SpreadsheetGear.Shapes.IShape chartShape = chartSheet.Shapes.AddChart(1, 1, 500, 500);
            SpreadsheetGear.Charts.IChart chart = chartShape.Chart;

            // working variables
            int lastRowIdx = dataWorksheet.UsedRange.RowCount;
            IRange xAxisColumn = dataWorksheet.Cells[1, 0, lastRowIdx-1, 0];
            IRange yAxisColumn = null;
            ISeries chartSeries = null;
            string seriesName = string.Empty;

            // Step 5.2: add a chart series for each Y axis column in the config
            foreach (var kvp in yAxisTargetColIdxs)
            {
                seriesName = dataWorksheet.Cells[0, kvp.Key].Text;
                yAxisColumn = dataWorksheet.Cells[1, kvp.Key, lastRowIdx-1, kvp.Key];

                chartSeries = chart.SeriesCollection.Add();
                chartSeries.XValues = $"={xAxisColumn.ToString()}"; // "Sheet1!$A2:$A200";
                chartSeries.Values = yAxisColumn.ToString();  //"Sheet1!$H2:$H200";
                chartSeries.ChartType = ChartType.Line;
                chartSeries.Name = seriesName;
            }

            // Step 5.3: format the chart title
            chart.HasTitle = true;
            StringBuilder chartTitle = new StringBuilder();
            chartTitle.AppendLine($"{graphConfig.Name}");
            chartTitle.AppendLine($"PID Gains: {dataWorksheet.Cells[1, pidGainsColumnIdx].Text}");
            chartTitle.AppendLine($"Follower Gains: {dataWorksheet.Cells[1, followerGainsColumnIdx].Text}");

            chart.ChartTitle.Text = chartTitle.ToString();
            chart.ChartTitle.Font.Size = 12;

            // Step 5.4: format the chart legend
            chart.Legend.Position = SpreadsheetGear.Charts.LegendPosition.Bottom;
            chart.Legend.Font.Bold = true;

            // Step 5.5: format X & Y Axes
            IAxis xAxis = chart.Axes[AxisType.Category];
            xAxis.HasMinorGridlines = true;
            xAxis.HasTitle = true;
            xAxis.TickMarkSpacing = 100;    // 10Msec per step * 100 = gidline every second
            IAxisTitle xAxisTitle = xAxis.AxisTitle;
            xAxisTitle.Text = graphConfig.XAxis.AxisTitle;

            IAxis yAxis = chart.Axes[AxisType.Value, AxisGroup.Primary];
            yAxis.HasTitle = true;
            IAxisTitle yAxisTitle = yAxis.AxisTitle;
            yAxisTitle.Text = graphConfig.YAxis.AxisTitle;
        }
    }
}
