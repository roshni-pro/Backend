using AngularJSAuthentication.API.Models;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace AngularJSAuthentication.API.Helpers
{
    public class TurnAroundTimeHelper
    {
        public static DataSet GenerateDataSet(TATInputModel input, List<TurnAroundTimeReportModel> turnAroundTimeReportModelList)
        {
            if (input.SPList != null && input.SPList.Count > 0)
            {
                turnAroundTimeReportModelList = turnAroundTimeReportModelList.Where(item => input.SPList.Any(item2 => item2 == item.Tablename)).ToList();
            }


            Dictionary<string, Object> paramList = new Dictionary<string, object>();
            paramList.Add("@StartDate", input.StartDate);
            paramList.Add("@EndDate", input.EndDate);
            paramList.Add("@WarehouseID", input.WarehouseID);

            DataSet dataSet = new DataSet();

            foreach (var sp in turnAroundTimeReportModelList)
            {
                DataTable dataTable = null;
                if (sp.Tablename == "AssignmentDashboardTAT-ReadyToDispatchToIssued")
                {
                    Dictionary<string, Object> paramList2 = new Dictionary<string, object>();
                    paramList2.Add("@StartDate", input.StartDate);
                    paramList2.Add("@EndDate", input.EndDate);
                    paramList2.Add("@WarehouseID", input.WarehouseID);
                    paramList2.Add("@SearchFilter", "ReadyToDispatchToIssued");
                    dataTable = GetDataTable(sp.ReportName, "AssignmentDashboardTAT", paramList2);
                }
                else if (sp.Tablename == "AssignmentDashboardTAT-IssuedToShipped")
                {
                    Dictionary<string, Object> paramList2 = new Dictionary<string, object>();
                    paramList2.Add("@StartDate", input.StartDate);
                    paramList2.Add("@EndDate", input.EndDate);
                    paramList2.Add("@WarehouseID", input.WarehouseID);
                    paramList2.Add("@SearchFilter", "IssuedToShipped");

                    dataTable = GetDataTable(sp.ReportName, "AssignmentDashboardTAT", paramList2);
                }
                else if (sp.Tablename == "TurnAroundTime")
                {
                    Dictionary<string, Object> paramList3 = new Dictionary<string, object>();
                    paramList3.Add("@StartDate", input.StartDate);
                    paramList3.Add("@EndDate", input.EndDate);
                    paramList3.Add("@WarehouseID", input.WarehouseID);
                    if (input.DboyMobileNo != null)
                    {
                        paramList3.Add("@DboyMobileNo", input.DboyMobileNo);
                    }

                    dataTable = GetDataTable(sp.ReportName, "TurnAroundTime", paramList3);
                }
                else
                {
                    dataTable = GetDataTable(sp.ReportName, sp.Tablename, paramList);

                }

                dataSet.Tables.Add(dataTable);
                sp.DataTable = dataTable;

            }

            //spNameList = new Dictionary<string, string>();
            //paramList.Add("@SearchFilter", "ReadyToDispatchToIssued");
            //DataTable dt = GetDataTable(spNameList.First().Value, spNameList.First().Key, paramList);
            //dataSet.Tables.Add(dt);

            //spNameList = new Dictionary<string, string>();
            //paramList.Add("@SearchFilter", "IssuedToShipped");
            //dt = GetDataTable(spNameList.First().Value, spNameList.First().Key, paramList);
            //dataSet.Tables.Add(dt);

            return dataSet;
        }

        public static void DataSet_To_Excel(DataSet dataSet, string pFilePath)
        {
            if (dataSet != null && dataSet.Tables.Count > 0)
            {
                IWorkbook workbook = null;
                ISheet worksheet = null;
                string Ext = System.IO.Path.GetExtension(pFilePath); //<-Extension del archivo
                switch (Ext.ToLower())
                {
                    case ".xls":
                        HSSFWorkbook workbookH = new HSSFWorkbook();
                        NPOI.HPSF.DocumentSummaryInformation dsi = NPOI.HPSF.PropertySetFactory.CreateDocumentSummaryInformation();
                        dsi.Company = "ShopKirana"; dsi.Manager = "IT Department";
                        workbookH.DocumentSummaryInformation = dsi;
                        workbook = workbookH;
                        break;

                    case ".xlsx": workbook = new XSSFWorkbook(); break;
                }
                using (FileStream stream = new FileStream(pFilePath, FileMode.Create, FileAccess.ReadWrite))
                {
                    foreach (DataTable pDatos in dataSet.Tables)
                    {
                        try
                        {
                            if (pDatos != null && pDatos.Rows.Count > 0)
                            {



                                worksheet = workbook.CreateSheet(pDatos.TableName); //<-Usa el nombre de la tabla como nombre de la Hoja


                                if (pDatos.Columns.Count > 0)
                                {
                                    int index = 0;
                                    foreach (var col in pDatos.Columns)
                                    {
                                        worksheet.SetColumnWidth(index++, (int)((22 + 0.72) * 256));
                                    }
                                }


                                //CREAR EN LA PRIMERA FILA LOS TITULOS DE LAS COLUMNAS
                                int iRow = 0;
                                if (pDatos.Columns.Count > 0)
                                {
                                    int iCol = 0;
                                    IRow fila = worksheet.CreateRow(iRow);
                                    foreach (DataColumn columna in pDatos.Columns)
                                    {
                                        ICell cell = fila.CreateCell(iCol, CellType.String);
                                        cell.SetCellValue(columna.ColumnName);
                                        iCol++;
                                    }
                                    iRow++;
                                }

                                //FORMATOS PARA CIERTOS TIPOS DE DATOS
                                ICellStyle _doubleCellStyle = workbook.CreateCellStyle();
                                _doubleCellStyle.DataFormat = workbook.CreateDataFormat().GetFormat("#,##0.###");

                                ICellStyle _intCellStyle = workbook.CreateCellStyle();
                                _intCellStyle.DataFormat = workbook.CreateDataFormat().GetFormat("#,##0");

                                ICellStyle _boolCellStyle = workbook.CreateCellStyle();
                                _boolCellStyle.DataFormat = workbook.CreateDataFormat().GetFormat("BOOLEAN");

                                ICellStyle _dateCellStyle = workbook.CreateCellStyle();
                                _dateCellStyle.DataFormat = workbook.CreateDataFormat().GetFormat("dd-MM-yyyy");

                                ICellStyle _dateTimeCellStyle = workbook.CreateCellStyle();
                                _dateTimeCellStyle.DataFormat = workbook.CreateDataFormat().GetFormat("dd-MM-yyyy HH:mm:ss");

                                //AHORA CREAR UNA FILA POR CADA REGISTRO DE LA TABLA
                                foreach (DataRow row in pDatos.Rows)
                                {
                                    IRow fila = worksheet.CreateRow(iRow);
                                    int iCol = 0;
                                    foreach (DataColumn column in pDatos.Columns)
                                    {
                                        ICell cell = null; //<-Representa la celda actual                               
                                        object cellValue = row[iCol]; //<- El valor actual de la celda

                                        switch (column.DataType.ToString())
                                        {
                                            case "System.Boolean":
                                                if (cellValue != DBNull.Value)
                                                {
                                                    cell = fila.CreateCell(iCol, CellType.Boolean);

                                                    if (Convert.ToBoolean(cellValue)) { cell.SetCellFormula("TRUE()"); }
                                                    else { cell.SetCellFormula("FALSE()"); }

                                                    cell.CellStyle = _boolCellStyle;
                                                }
                                                break;

                                            case "System.String":
                                                if (cellValue != DBNull.Value)
                                                {
                                                    cell = fila.CreateCell(iCol, CellType.String);
                                                    cell.SetCellValue(Convert.ToString(cellValue));
                                                }
                                                break;

                                            case "System.Int32":
                                                if (cellValue != DBNull.Value)
                                                {
                                                    cell = fila.CreateCell(iCol, CellType.Numeric);
                                                    cell.SetCellValue(Convert.ToInt32(cellValue));
                                                    //cell.CellStyle = _intCellStyle;
                                                }
                                                break;
                                            case "System.Int64":
                                                if (cellValue != DBNull.Value)
                                                {
                                                    cell = fila.CreateCell(iCol, CellType.Numeric);
                                                    cell.SetCellValue(Convert.ToInt64(cellValue));
                                                    cell.CellStyle = _intCellStyle;
                                                }
                                                break;
                                            case "System.Decimal":
                                                if (cellValue != DBNull.Value)
                                                {
                                                    cell = fila.CreateCell(iCol, CellType.Numeric);
                                                    cell.SetCellValue(Convert.ToDouble(cellValue));
                                                    cell.CellStyle = _doubleCellStyle;
                                                }
                                                break;
                                            case "System.Double":
                                                if (cellValue != DBNull.Value)
                                                {
                                                    cell = fila.CreateCell(iCol, CellType.Numeric);
                                                    cell.SetCellValue(Convert.ToDouble(cellValue));
                                                    cell.CellStyle = _doubleCellStyle;
                                                }
                                                break;

                                            case "System.DateTime":
                                                if (cellValue != DBNull.Value)
                                                {
                                                    cell = fila.CreateCell(iCol, CellType.Numeric);
                                                    cell.SetCellValue(Convert.ToDateTime(cellValue));

                                                    //Si No tiene valor de Hora, usar formato dd-MM-yyyy
                                                    DateTime cDate = Convert.ToDateTime(cellValue);
                                                    if (cDate != null && cDate.Hour > 0) { cell.CellStyle = _dateTimeCellStyle; }
                                                    else { cell.CellStyle = _dateCellStyle; }
                                                }
                                                break;
                                            default:
                                                break;
                                        }
                                        iCol++;
                                    }
                                    iRow++;
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                            throw ex;
                        }
                    }
                    workbook.Write(stream);
                    stream.Close();
                }
            }
        }

        private static DataTable GetDataTable(string tableName, string spName, Dictionary<string, Object> paramList)
        {
            var table = new DataTable();
            table.TableName = tableName;
            string connectionString = ConfigurationManager.ConnectionStrings["authcontext"].ToString();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand sqlCommand = new SqlCommand();
                sqlCommand.Connection = connection;
                sqlCommand.CommandText = spName;
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCommand.CommandTimeout = 300;

                if (paramList != null && paramList.Count > 0)
                {
                    foreach (var item in paramList)
                    {
                        sqlCommand.Parameters.Add(new SqlParameter(item.Key, item.Value));
                    }
                }
                try
                {

                    connection.Open();
                    SqlDataAdapter da = new SqlDataAdapter(sqlCommand);
                    // Execute the stored procedure.
                    da.Fill(table);

                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    // Close the connection.
                    connection.Close();

                }
                return table;

            }
        }

        public static List<TurnAroundTimeReportModel> GetTurnAroundTimeReportModelList()
        {
            List<TurnAroundTimeReportModel> turnAroundTimeReportModelList = new List<TurnAroundTimeReportModel>();
            turnAroundTimeReportModelList.Add(new TurnAroundTimeReportModel
            {
                DataTable = null,
                PageTitle = "Turn Around Time Report",
                ReportName = "TAT Sheet",
                Tablename = "TurnAroundTime"
            });
            turnAroundTimeReportModelList.Add(new TurnAroundTimeReportModel
            {
                DataTable = null,
                PageTitle = "Pending - Reday to Dispatch Report",
                ReportName = "Order Dispatch TAT Sheet",
                Tablename = "PendingToReadyToDispatchTAT"
            });
            turnAroundTimeReportModelList.Add(new TurnAroundTimeReportModel
            {
                DataTable = null,
                PageTitle = "Ready to Dispatch - issued	AND	Issue - shipped",
                ReportName = "Assignment TAT Sheet",
                Tablename = "ReadyToDispatchToShippedTAT"
            });
            turnAroundTimeReportModelList.Add(new TurnAroundTimeReportModel
            {
                DataTable = null,
                PageTitle = "Shipped - Delivery",
                ReportName = "Delivery TAT Sheet",
                Tablename = "ShippedToDelivertTAT"
            });
            turnAroundTimeReportModelList.Add(new TurnAroundTimeReportModel
            {
                DataTable = null,
                PageTitle = "Orderdispatch TAT Report",
                ReportName = "Order Dispatch Dashboard",
                Tablename = "OrderDispatchDashboardTAT"
            });
            turnAroundTimeReportModelList.Add(new TurnAroundTimeReportModel
            {
                DataTable = null,
                PageTitle = "Assignment TAT Two Report",
                ReportName = "Assignment Dashboard Two Sheet",
                Tablename = "AssignmentDashboardTwoTAT"
            });
            turnAroundTimeReportModelList.Add(new TurnAroundTimeReportModel
            {
                DataTable = null,
                PageTitle = "Delivery Dashboard Report",
                ReportName = "Delivery Dashboard",
                Tablename = "DeliveryDashboardTAT"
            });
            turnAroundTimeReportModelList.Add(new TurnAroundTimeReportModel
            {
                DataTable = null,
                PageTitle = "Assignment Dashboard One (Ready to Dispatch - Issued)",
                ReportName = "Assignment Dashboard One SheetReadyToDispatchToIssued",
                Tablename = "AssignmentDashboardTAT-ReadyToDispatchToIssued"
            });
            turnAroundTimeReportModelList.Add(new TurnAroundTimeReportModel
            {
                DataTable = null,
                PageTitle = "Assignment Dashboard One (Issued - Shipped)",
                ReportName = "Assignment Dashboard One SheetIssuedToShipped",
                Tablename = "AssignmentDashboardTAT-IssuedToShipped"
            });
            return turnAroundTimeReportModelList;
        }

        public List<int> GetActiveWarehouseList()
        {
            List<int> warehouses = null;
            using(var authContext = new AuthContext())
            {
                warehouses = authContext.Warehouses.Where(x => x.active == true && x.Deleted == false).Select(y => y.WarehouseId).ToList();
            }
            return warehouses;
        }
    }
}