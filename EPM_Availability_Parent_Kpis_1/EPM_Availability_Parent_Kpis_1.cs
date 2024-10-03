namespace EPM_Availability_All_Endpoints_1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Skyline.DataMiner.Analytics.GenericInterface;
    using Skyline.DataMiner.Net;
    using Skyline.DataMiner.Net.Messages;

    /// <summary>
    /// Represents a DataMiner Automation script.
    /// </summary>
    [GQIMetaData(Name = "All Parent Kpi Data")]
    public class CmData : IGQIDataSource, IGQIInputArguments, IGQIOnInit
    {
        private readonly GQIStringArgument frontEndElementArg = new GQIStringArgument("FE Element")
        {
            IsRequired = true,
        };

        private readonly GQIStringArgument systemTypeArg = new GQIStringArgument("System Type")
        {
            IsRequired = false,
        };

        private readonly GQIStringArgument systemNameArg = new GQIStringArgument("System Name")
        {
            IsRequired = false,
        };

        private GQIDMS _dms;

        private string frontEndElement = String.Empty;

        private string systemType = String.Empty;

        private string systemName = String.Empty;

        private List<GQIRow> listGqiRows = new List<GQIRow> { };

        private Tuple<int, string> systemTypeFilter = new Tuple<int, string>(-1, String.Empty);

        public OnInitOutputArgs OnInit(OnInitInputArgs args)
        {
            _dms = args.DMS;
            return new OnInitOutputArgs();
        }

        public GQIArgument[] GetInputArguments()
        {
            return new GQIArgument[]
            {
                frontEndElementArg,
                systemTypeArg,
                systemNameArg,
            };
        }

        public GQIColumn[] GetColumns()
        {
            return new GQIColumn[]
            {
                new GQIStringColumn("Name"),
                new GQIIntColumn("Number Endpoints"),
                new GQIDoubleColumn("Percentage Unreachable"),
                new GQIDoubleColumn("Average Packet Loss Rate"),
                new GQIDoubleColumn("Average Jitter"),
                new GQIDoubleColumn("Average Latency"),
                new GQIDoubleColumn("Average RTT"),
            };
        }

        public GQIPage GetNextPage(GetNextPageInputArgs args)
        {

            return new GQIPage(listGqiRows.ToArray())
            {
                HasNextPage = false,
            };

            //try
            //{
            //    listGqiRows.Clear();

            //    if (iterator == allCollectors.Count)
            //    {
            //        return new GQIPage(listGqiRows.ToArray())
            //        {
            //            HasNextPage = false,
            //        };
            //    }
            //    else
            //    {
            //        var collectorRows = GetTable(allCollectors[iterator], 2000, new List<string>
            //        {
            //            systemTypeFilter,
            //        });

            //        Dictionary<string, EndpointOverview> endpointRows = ExtractCollectorData(collectorRows);
            //        AddAllCableModems(endpointRows);

            //        iterator++;

            //        return new GQIPage(listGqiRows.ToArray())
            //        {
            //            HasNextPage = true,
            //        };
            //    }
            //}
            //catch
            //{
            //    return new GQIPage(listGqiRows.ToArray())
            //    {
            //        HasNextPage = false,
            //    };
            //}
        }

        public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
        {
            listGqiRows.Clear();
            try
            {
                frontEndElement = args.GetArgumentValue(frontEndElementArg);
                systemType = args.GetArgumentValue(systemTypeArg);
                systemName = args.GetArgumentValue(systemNameArg);

                systemTypeFilter = GetSystemTypeFilter();
                if (String.IsNullOrEmpty(systemTypeFilter.Item2))
                {
                    return new OnArgumentsProcessedOutputArgs();
                }

                var kpiRow = GetTable(frontEndElement, systemTypeFilter.Item1, new List<string>
                {
                    systemTypeFilter.Item2,
                });

                Dictionary<string, KpisOverview> parentKpis = ExtractKpis(kpiRow);
                AddAllKpis(parentKpis);
            }
            catch
            {
                listGqiRows = new List<GQIRow>();
            }

            return new OnArgumentsProcessedOutputArgs();
        }

        public Tuple<int, string> GetSystemTypeFilter()
        {
            switch (systemType)
            {
                case "Customer":
                    return new Tuple<int, string>(3500, String.Format("fullFilter=(3502=={0});columns={1},{2},{3},{4},{5},{6},{7}", systemName, 3502, 3503, 3505, 3513, 3509, 3511, 3507));
                case "Vendor":
                    return new Tuple<int, string>(4500, String.Format("fullFilter=(4502=={0});columns={1},{2},{3},{4},{5},{6},{7}", systemName, 4502, 4503, 4505, 4513, 4509, 4511, 4507));
                case "Network":
                    return new Tuple<int, string>(9500, String.Format("fullFilter=(9502=={0});columns={1},{2},{3},{4},{5},{6},{7}", systemName, 9502, 9503, 9505, 9513, 9509, 9511, 9507));
                case "Region":
                    return new Tuple<int, string>(8500, String.Format("fullFilter=(8502=={0});columns={1},{2},{3},{4},{5},{6},{7}", systemName, 8502, 8504, 8506, 8514, 8510, 8512, 8508));
                case "Sub-Region":
                    return new Tuple<int, string>(7500, String.Format("fullFilter=(7502=={0});columns={1},{2},{3},{4},{5},{6},{7}", systemName, 7502, 7505, 7507, 7515, 7511, 7513, 7509));
                case "Hub":
                    return new Tuple<int, string>(6500, String.Format("fullFilter=(6502=={0});columns={1},{2},{3},{4},{5},{6},{7}", systemName, 6502, 6506, 6508, 6516, 6512, 6514, 6510));
                case "Station":
                    return new Tuple<int, string>(5500, String.Format("fullFilter=(5502=={0});columns={1},{2},{3},{4},{5},{6},{7}", systemName, 5502, 5507, 5509, 5517, 5513, 5515, 5511));
                default:
                    return new Tuple<int, string>(-1, String.Empty);
            }
        }

        public GenIfRowMetadata GetSystemTypeMetaData(string key)
        {
            var dmaIdParts = frontEndElement.Split('/');
            ObjectRefMetadata unreachableMetadata;
            ObjectRefMetadata avgPacketLossMetadata;
            ObjectRefMetadata avgJitterMetadata;
            ObjectRefMetadata avgLatencyMetadata;
            ObjectRefMetadata avgRttMetadata;

            switch (systemType)
            {
                case "Customer":
                    unreachableMetadata = new ObjectRefMetadata { Object = new ParamID(Convert.ToInt32(dmaIdParts[0]), Convert.ToInt32(dmaIdParts[1]), 3505, key) };
                    avgPacketLossMetadata = new ObjectRefMetadata { Object = new ParamID(Convert.ToInt32(dmaIdParts[0]), Convert.ToInt32(dmaIdParts[1]), 3513, key) };
                    avgJitterMetadata = new ObjectRefMetadata { Object = new ParamID(Convert.ToInt32(dmaIdParts[0]), Convert.ToInt32(dmaIdParts[1]), 3509, key) };
                    avgLatencyMetadata = new ObjectRefMetadata { Object = new ParamID(Convert.ToInt32(dmaIdParts[0]), Convert.ToInt32(dmaIdParts[1]), 3511, key) };
                    avgRttMetadata = new ObjectRefMetadata { Object = new ParamID(Convert.ToInt32(dmaIdParts[0]), Convert.ToInt32(dmaIdParts[1]), 3507, key) };
                    return new GenIfRowMetadata(new[] { unreachableMetadata, avgPacketLossMetadata, avgJitterMetadata, avgLatencyMetadata, avgRttMetadata });
                case "Vendor":
                    unreachableMetadata = new ObjectRefMetadata { Object = new ParamID(Convert.ToInt32(dmaIdParts[0]), Convert.ToInt32(dmaIdParts[1]), 4505, key) };
                    avgPacketLossMetadata = new ObjectRefMetadata { Object = new ParamID(Convert.ToInt32(dmaIdParts[0]), Convert.ToInt32(dmaIdParts[1]), 4513, key) };
                    avgJitterMetadata = new ObjectRefMetadata { Object = new ParamID(Convert.ToInt32(dmaIdParts[0]), Convert.ToInt32(dmaIdParts[1]), 4509, key) };
                    avgLatencyMetadata = new ObjectRefMetadata { Object = new ParamID(Convert.ToInt32(dmaIdParts[0]), Convert.ToInt32(dmaIdParts[1]), 4511, key) };
                    avgRttMetadata = new ObjectRefMetadata { Object = new ParamID(Convert.ToInt32(dmaIdParts[0]), Convert.ToInt32(dmaIdParts[1]), 4507, key) };
                    return new GenIfRowMetadata(new[] { unreachableMetadata, avgPacketLossMetadata, avgJitterMetadata, avgLatencyMetadata, avgRttMetadata });
                case "Network":
                    unreachableMetadata = new ObjectRefMetadata { Object = new ParamID(Convert.ToInt32(dmaIdParts[0]), Convert.ToInt32(dmaIdParts[1]), 9505, key) };
                    avgPacketLossMetadata = new ObjectRefMetadata { Object = new ParamID(Convert.ToInt32(dmaIdParts[0]), Convert.ToInt32(dmaIdParts[1]), 9513, key) };
                    avgJitterMetadata = new ObjectRefMetadata { Object = new ParamID(Convert.ToInt32(dmaIdParts[0]), Convert.ToInt32(dmaIdParts[1]), 9509, key) };
                    avgLatencyMetadata = new ObjectRefMetadata { Object = new ParamID(Convert.ToInt32(dmaIdParts[0]), Convert.ToInt32(dmaIdParts[1]), 9511, key) };
                    avgRttMetadata = new ObjectRefMetadata { Object = new ParamID(Convert.ToInt32(dmaIdParts[0]), Convert.ToInt32(dmaIdParts[1]), 9507, key) };
                    return new GenIfRowMetadata(new[] { unreachableMetadata, avgPacketLossMetadata, avgJitterMetadata, avgLatencyMetadata, avgRttMetadata });
                case "Region":
                    unreachableMetadata = new ObjectRefMetadata { Object = new ParamID(Convert.ToInt32(dmaIdParts[0]), Convert.ToInt32(dmaIdParts[1]), 8506, key) };
                    avgPacketLossMetadata = new ObjectRefMetadata { Object = new ParamID(Convert.ToInt32(dmaIdParts[0]), Convert.ToInt32(dmaIdParts[1]), 8514, key) };
                    avgJitterMetadata = new ObjectRefMetadata { Object = new ParamID(Convert.ToInt32(dmaIdParts[0]), Convert.ToInt32(dmaIdParts[1]), 8510, key) };
                    avgLatencyMetadata = new ObjectRefMetadata { Object = new ParamID(Convert.ToInt32(dmaIdParts[0]), Convert.ToInt32(dmaIdParts[1]), 8512, key) };
                    avgRttMetadata = new ObjectRefMetadata { Object = new ParamID(Convert.ToInt32(dmaIdParts[0]), Convert.ToInt32(dmaIdParts[1]), 8508, key) };
                    return new GenIfRowMetadata(new[] { unreachableMetadata, avgPacketLossMetadata, avgJitterMetadata, avgLatencyMetadata, avgRttMetadata });
                case "Sub-Region":
                    unreachableMetadata = new ObjectRefMetadata { Object = new ParamID(Convert.ToInt32(dmaIdParts[0]), Convert.ToInt32(dmaIdParts[1]), 7507, key) };
                    avgPacketLossMetadata = new ObjectRefMetadata { Object = new ParamID(Convert.ToInt32(dmaIdParts[0]), Convert.ToInt32(dmaIdParts[1]), 7515, key) };
                    avgJitterMetadata = new ObjectRefMetadata { Object = new ParamID(Convert.ToInt32(dmaIdParts[0]), Convert.ToInt32(dmaIdParts[1]), 7511, key) };
                    avgLatencyMetadata = new ObjectRefMetadata { Object = new ParamID(Convert.ToInt32(dmaIdParts[0]), Convert.ToInt32(dmaIdParts[1]), 7513, key) };
                    avgRttMetadata = new ObjectRefMetadata { Object = new ParamID(Convert.ToInt32(dmaIdParts[0]), Convert.ToInt32(dmaIdParts[1]), 7509, key) };
                    return new GenIfRowMetadata(new[] { unreachableMetadata, avgPacketLossMetadata, avgJitterMetadata, avgLatencyMetadata, avgRttMetadata });
                case "Hub":
                    unreachableMetadata = new ObjectRefMetadata { Object = new ParamID(Convert.ToInt32(dmaIdParts[0]), Convert.ToInt32(dmaIdParts[1]), 6508, key) };
                    avgPacketLossMetadata = new ObjectRefMetadata { Object = new ParamID(Convert.ToInt32(dmaIdParts[0]), Convert.ToInt32(dmaIdParts[1]), 6516, key) };
                    avgJitterMetadata = new ObjectRefMetadata { Object = new ParamID(Convert.ToInt32(dmaIdParts[0]), Convert.ToInt32(dmaIdParts[1]), 6512, key) };
                    avgLatencyMetadata = new ObjectRefMetadata { Object = new ParamID(Convert.ToInt32(dmaIdParts[0]), Convert.ToInt32(dmaIdParts[1]), 6514, key) };
                    avgRttMetadata = new ObjectRefMetadata { Object = new ParamID(Convert.ToInt32(dmaIdParts[0]), Convert.ToInt32(dmaIdParts[1]), 6510, key) };
                    return new GenIfRowMetadata(new[] { unreachableMetadata, avgPacketLossMetadata, avgJitterMetadata, avgLatencyMetadata, avgRttMetadata });
                case "Station":
                    unreachableMetadata = new ObjectRefMetadata { Object = new ParamID(Convert.ToInt32(dmaIdParts[0]), Convert.ToInt32(dmaIdParts[1]), 5509, key) };
                    avgPacketLossMetadata = new ObjectRefMetadata { Object = new ParamID(Convert.ToInt32(dmaIdParts[0]), Convert.ToInt32(dmaIdParts[1]), 5517, key) };
                    avgJitterMetadata = new ObjectRefMetadata { Object = new ParamID(Convert.ToInt32(dmaIdParts[0]), Convert.ToInt32(dmaIdParts[1]), 5513, key) };
                    avgLatencyMetadata = new ObjectRefMetadata { Object = new ParamID(Convert.ToInt32(dmaIdParts[0]), Convert.ToInt32(dmaIdParts[1]), 5515, key) };
                    avgRttMetadata = new ObjectRefMetadata { Object = new ParamID(Convert.ToInt32(dmaIdParts[0]), Convert.ToInt32(dmaIdParts[1]), 5511, key) };
                    return new GenIfRowMetadata(new[] { unreachableMetadata, avgPacketLossMetadata, avgJitterMetadata, avgLatencyMetadata, avgRttMetadata });
                default:
                    return new GenIfRowMetadata(new ObjectRefMetadata[] { });
            }
        }

        public List<HelperPartialSettings[]> GetTable(string element, int tableId, List<string> filter)
        {
            var columns = new List<HelperPartialSettings[]>();

            var elementIds = element.Split('/');
            if (elementIds.Length > 1 && Int32.TryParse(elementIds[0], out int dmaId) && Int32.TryParse(elementIds[1], out int elemId))
            {
                // Retrieve client connections from the DMS using a GetInfoMessage request
                var getPartialTableMessage = new GetPartialTableMessage(dmaId, elemId, tableId, filter.ToArray());
                var paramChange = (ParameterChangeEventMessage)_dms.SendMessage(getPartialTableMessage);

                if (paramChange != null && paramChange.NewValue != null && paramChange.NewValue.ArrayValue != null)
                {
                    columns = paramChange.NewValue.ArrayValue
                        .Where(av => av != null && av.ArrayValue != null)
                        .Select(p => p.ArrayValue.Where(v => v != null)
                        .Select(c => new HelperPartialSettings
                        {
                            CellValue = c.CellValue.InteropValue,
                            DisplayValue = c.CellValue.CellDisplayValue,
                            DisplayType = c.CellDisplayState,
                        }).ToArray()).ToList();
                }
            }

            return columns;
        }

        public static string ParseDoubleValue(double doubleValue, string unit)
        {
            if (doubleValue.Equals(-1))
            {
                return "N/A";
            }

            return Math.Round(doubleValue, 2) + " " + unit;
        }

        public static string ParseStringValue(string stringValue)
        {
            if (String.IsNullOrEmpty(stringValue) || stringValue == "-1")
            {
                return "N/A";
            }

            return stringValue;
        }

        private static Dictionary<string, KpisOverview> ExtractKpis(List<HelperPartialSettings[]> kpiRow)
        {
            Dictionary<string, KpisOverview> endpointRows = new Dictionary<string, KpisOverview>();
            if (kpiRow != null && kpiRow.Any())
            {
                for (int i = 0; i < kpiRow[0].Count(); i++)
                {
                    var key = Convert.ToString(kpiRow[0][i].CellValue);
                    var oltRow = new KpisOverview
                    {
                        Key = key,
                        Name = Convert.ToString(kpiRow[1][i].CellValue),
                        NumberEndpoints = Convert.ToInt32(kpiRow[2][i].CellValue),
                        UnreachableEndpoints = Convert.ToDouble(kpiRow[3][i].CellValue),
                        AveragePacketLossRate = Convert.ToDouble(kpiRow[4][i].CellValue),
                        AverageJitter = Convert.ToDouble(kpiRow[5][i].CellValue),
                        AverageLatency = Convert.ToDouble(kpiRow[6][i].CellValue),
                        AverageRtt = Convert.ToDouble(kpiRow[7][i].CellValue),
                    };

                    endpointRows[key] = oltRow;
                }
            }

            return endpointRows;
        }

        private void AddAllKpis(Dictionary<string, KpisOverview> kpiRows)
        {
            foreach (var kpiRow in kpiRows.Values)
            {
                List<GQICell> listGqiCells = new List<GQICell>
                {
                    new GQICell
                    {
                        Value = kpiRow.Name,
                    },
                    new GQICell
                    {
                        Value = kpiRow.NumberEndpoints,
                        DisplayValue = ParseDoubleValue(kpiRow.NumberEndpoints, String.Empty),
                    },
                    new GQICell
                    {
                        Value = kpiRow.UnreachableEndpoints,
                        DisplayValue = ParseDoubleValue(kpiRow.UnreachableEndpoints, "%"),
                    },
                    new GQICell
                    {
                        Value = kpiRow.AveragePacketLossRate,
                        DisplayValue = ParseDoubleValue(kpiRow.AveragePacketLossRate, "%"),
                    },
                    new GQICell
                    {
                        Value = kpiRow.AverageJitter,
                        DisplayValue = ParseDoubleValue(kpiRow.AverageJitter, "ms"),
                    },
                    new GQICell
                    {
                        Value = kpiRow.AverageLatency,
                        DisplayValue = ParseDoubleValue(kpiRow.AverageLatency, "ms"),
                    },
                    new GQICell
                    {
                        Value = kpiRow.AverageRtt,
                        DisplayValue = ParseDoubleValue(kpiRow.AverageRtt, "ms"),
                    },
                };

                var rowMetadata = GetSystemTypeMetaData(kpiRow.Key);
                var gqiRow = new GQIRow(listGqiCells.ToArray()) { Metadata = rowMetadata };

                listGqiRows.Add(gqiRow);
            }
        }
    }

    public class BackEndHelper
    {
        public string ElementId { get; set; }

        public string OLtId { get; set; }

        public string EntityId { get; set; }
    }

    public class HelperPartialSettings
    {
        public object CellValue { get; set; }

        public object DisplayValue { get; set; }

        public ParameterDisplayType DisplayType { get; set; }
    }

    public class KpisOverview
    {
        public string Key { get; set; }

        public string Name { get; set; }

        public int NumberEndpoints { get; set; }

        public double UnreachableEndpoints { get; set; }

        public double AveragePacketLossRate { get; set; }

        public double AverageJitter { get; set; }

        public double AverageLatency { get; set; }

        public double AverageRtt { get; set; }
    }
}