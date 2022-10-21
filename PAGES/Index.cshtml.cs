using BarcodeLib;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Npgsql;
using RawPrinterHelperPage;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Text;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Rest.Verify.V2.Service.Entity;
using static Test_InventoryPage.ViewModels;

namespace Test_InventoryPage.Pages
{

    [ApiController]
    [Route("[controller]/[action]")]
    public class Index : ControllerBase
    {

        private readonly ILogger<Index> _logger;
        private  Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment;

        public Index(ILogger<Index> logger, Microsoft.AspNetCore.Hosting.IHostingEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }


        //private readonly ILogger<Index> _logger;


        //private readonly IHostingEnvironment _environment;

        //public Index(ILogger<Index> logger)
        //{
        //    _logger = logger;

        //}


        Barcode barcode = new Barcode();


        public double grandTotal { get; set; }
        public double invoice { get; set; }
        public string personnelCode { get; set; }

        [HttpGet]
        public dynamic GetInventoryList(string? storeId)
        {
            List<InventoryViewModel> list = new List<InventoryViewModel>();
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();
            var query = "";

            if (storeId == "undefined")
            {
                query = @"select pp.id,title,department, pd.id as departmantid ,category, pc.id as categoryid ,max(instock),max(inrepair),pp.storeid,ps.description ,max(ebayprice),max(amazonprice),max(sellprice )
													from vw_product pp 
                                                    left join  department pd on pd.description = pp.department 
                                                    left join  category pc on pc.description = pp.category 
                                                    join  store ps  on ps.id = pp.storeid 
                                                    join store_group sg on sg.id  = ps.storegroupid 
                                                    where 
                                                    1=1
                                                    and sg.id = (select storegroupid from store where case when :storeId is null then id is null else id = :storeId end)  
                                                    group by pp.id,title,department, pd.id  ,category, pc.id ,pp.storeid,ps.description , ps.id 
                                                    order by case when ps.id = :storeId then ps.id  end";
            }
            else if (storeId == "0")
            {
                query = @"select pp.id,title,department, pd.id as departmantid ,category, pc.id as categoryid ,instock,inrepair,pp.storeid,ps.description ,ebayprice,amazonprice,sellprice 
													from vw_product pp 
                                                    left join  department pd on pd.description = pp.department 
                                                    left join  category pc on pc.description = pp.category 
                                                    join  store ps  on ps.id = pp.storeid";
            }
            else
            {
                query = @"select pp.id,title,department, pd.id as departmantid ,category, pc.id as categoryid ,max(instock),max(inrepair),pp.storeid,ps.description ,max(ebayprice),max(amazonprice),max(sellprice )
													from vw_product pp 
                                                    left join  department pd on pd.description = pp.department 
                                                    left join  category pc on pc.description = pp.category 
                                                    join  store ps  on ps.id = pp.storeid 
                                                    join store_group sg on sg.id  = ps.storegroupid 
                                                    where 
                                                    1=1
                                                    and sg.id = (select storegroupid from store where case when :storeId is null then id is null else id = :storeId end)  
                                                    group by pp.id,title,department, pd.id  ,category, pc.id ,pp.storeid,ps.description , ps.id 
                                                    order by case when ps.id = :storeId then ps.id  end";
            }
            using (var cmd = new NpgsqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("storeId", storeId == "undefined" ? 0 : Convert.ToDouble(storeId) );
                //cmd.Prepare();
                NpgsqlDataReader reader = cmd.ExecuteReader();


                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        list.Add(new InventoryViewModel { 
                            itemId = reader[0]?.ToString(),
                            itemName = reader[1]?.ToString(),
                            department = reader[2]?.ToString(),
                            departmentId = Convert.ToDouble(reader[3] == DBNull.Value ? 0 : reader[3]),
                            category = reader[4]?.ToString(),
                            categoryId = Convert.ToDouble(reader[5] == DBNull.Value ? 0 : reader[5]),
                            inStock = Convert.ToDouble(reader[6] == DBNull.Value ? 0 : reader[6]),
                            inRepair = Convert.ToDouble(reader[7] == DBNull.Value ? 0 : reader[7]),
                            storeId = Convert.ToDouble(reader[8] == DBNull.Value ? 0 : reader[8]),
                            storeName = reader[9]?.ToString(),
                            ebayPrice = Convert.ToDouble(reader[10] == DBNull.Value ? 0 : reader[10]),
                            amazonPrice = Convert.ToDouble(reader[11] == DBNull.Value ? 0 : reader[11]),
                            sellPrice = Convert.ToDouble(reader[12] == DBNull.Value ? 0 : reader[12]) });
                    }
                }
                else
                {
                }
                reader.Close();


            }
            connection.Close();

            return list ;

        }


        [HttpGet]
        public dynamic GetProductRequestList(string? cashierId)
        {
            List<GetProductRequestViewModel> getProductRequestViewModel = new List<GetProductRequestViewModel>();
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@"with data as (select  vp.id , vp.title ,vp.sellprice from vw_product vp 
                                                    where 
                                                    vp.storeid = (select storeid  from  personnel pp where pp.personnelcode = :cashierId) or vp.storeid is null                                                     
                                                    and instock < 0 or instock is null)
                                                    select * from data where data.id is not null", connection))
            {
                cmd.Parameters.AddWithValue("cashierId", cashierId?.ToString() == null ? DBNull.Value.ToString() : cashierId?.ToString());
                cmd.Prepare();
                NpgsqlDataReader reader = cmd.ExecuteReader();


                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        getProductRequestViewModel.Add(new GetProductRequestViewModel
                        {
                            itemId = reader[0]?.ToString(),
                            itemName = reader[1]?.ToString(),
                            sellPrice = Convert.ToDouble(reader[2] == DBNull.Value ? 0 : reader[2])
                        });
                    }
                }
                else
                {
                }
                reader.Close();


            }
            connection.Close();

            return getProductRequestViewModel;

        }


        [HttpGet]
        public dynamic GetBarcodeList(double? storeId)
        {
            List<InventoryBarcodeViewModel> inventoryBarcodeViewModel = new List<InventoryBarcodeViewModel>();
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@"select itemid ,itemname , ps.description , sellprice , storeid 
                                                    from  product pp 
                                                    join  store ps on ps.id = pp.storeid 
                                                    where case when :storeId = 0 then storeid is not null else storeid = :storeId end 
                                                    union all                                                     
                                                    select  vp.id  , title ,ps.description , sellprice , storeid  from vw_product vp 
                                                    join  store ps on ps.id = vp.storeid    
                                                    where case when :storeId = 0 then storeid is not null else storeid = :storeId end", connection))
            {
                cmd.Parameters.AddWithValue("storeId", storeId );
                cmd.Prepare();
                NpgsqlDataReader reader = cmd.ExecuteReader();
                Barcode barcode = new Barcode();
                Random generator = new Random();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        inventoryBarcodeViewModel.Add(new InventoryBarcodeViewModel
                        {
                            itemId = reader[0]?.ToString(),
                            itemName = reader[1]?.ToString(),
                            storeName = reader[2]?.ToString(),
                            sellPrice = Convert.ToDouble(reader[3] == DBNull.Value ? 0 : reader[3]),
                            storeId = Convert.ToDouble(reader[4] == DBNull.Value ? 0 : reader[4]),
                            productBarcode = 
                            String.Format("data:image/jpg;base64,{0}",
                            Convert.ToBase64String(
                            ConvertImageToByte(
                            barcode.Encode(BarcodeLib.TYPE.EAN13, 
                            reader[0].ToString()+generator.Next(0, 0000000).ToString("D" + (13 - reader[0]?.ToString()?.Length).ToString()), 
                            Color.Black, Color.White, 290, 120)))),
                        }) ;
                    }
                }
                else
                {
                }
                reader.Close();


            }
            connection.Close();

            return inventoryBarcodeViewModel;

        }


        [HttpGet]
        public List<ItemInfoViewModel> GetItemInfo(string? itemId , double storeId)
        {
            List<ItemInfoViewModel> list = new List<ItemInfoViewModel>();

            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@"select 
                                                    id ,
                                                    title::text || '-' || category ,
                                                    vp.sellprice 
                                                    from vw_product vp 
                                                    where case when :itemId = 'AllData' then id is not null else id = :itemId end                                                    
                                                    and storeid = :storeId 
                                                    and title != ''
                                                    and instock > 0 ", connection))
            {
                cmd.Parameters.AddWithValue("itemId", itemId == null ? "AllData" : itemId);
                cmd.Parameters.AddWithValue("storeId", storeId == 0 ? 0 : storeId);
                cmd.Prepare();
                NpgsqlDataReader reader = cmd.ExecuteReader();


                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        list.Add(new ItemInfoViewModel
                        {
                            itemId = reader[0]?.ToString(),
                            itemName = reader[1]?.ToString(),
                            sellPrice = Convert.ToDouble(reader[2] == DBNull.Value ? 0 : reader[2])                        
                        });
                    }
                }
                else
                {
                }
                reader.Close();


            }
            connection.Close();

            return list;

        }

        
        [HttpGet]
        public List<SaleInfoViewModel> GetSaleInfo(double storeId)
        {
            List<SaleInfoViewModel> list = new List<SaleInfoViewModel>();

            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@"(select 
                                            itemid, 
                                            itemname, 
                                            tradein, 
                                            quantity, 
                                            isdiscount, 
                                            sellprice,
                                            storeid,
                                            torepair,
                                            s.id,
                                            sellprice + sellprice * ps.taxrate /100 as grandtotal
                                            from sales s 
                                            join  store ps on ps.id = s.storeid 
                                            where 
                                            case when :storeId is null then storeid is not null else storeid = :storeId end and
                                            sellprice > 0
                                            order by itemid  asc)
                                            union all
                                            (select 
                                            itemid, 
                                            itemname, 
                                            tradein, 
                                            quantity, 
                                            isdiscount, 
                                            sellprice,
                                            storeid,
                                            torepair,
                                            s.id ,
                                            sellprice as grandtotal
                                            from sales s 
                                            join  store ps on ps.id = s.storeid 
                                            where 
                                            case when :storeId is null then storeid is not null else storeid = :storeId end and
                                            sellprice < 0
                                            order by itemid  asc)  ", connection))
            {
                cmd.Parameters.AddWithValue("storeId", storeId == null ? DBNull.Value : storeId);
                NpgsqlDataReader reader = cmd.ExecuteReader();


                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        list.Add(new SaleInfoViewModel
                        {
                            itemId = reader[0]?.ToString(),
                            itemName = reader[1]?.ToString(),
                            tradeIn = Convert.ToBoolean(reader[2] == DBNull.Value ? 0 : reader[2]),
                            quantity = Convert.ToDouble(reader[3] == DBNull.Value ? 0 : reader[3]),
                            isDiscount = Convert.ToBoolean(reader[4] == DBNull.Value ? 0 : reader[4]),
                            sellPrice = Convert.ToDouble(reader[5] == DBNull.Value ? 0 : reader[5]),
                            storeId = Convert.ToDouble(reader[6] == DBNull.Value ? 0 : reader[6]),
                            toRepair = Convert.ToBoolean(reader[7] == DBNull.Value ? 0 : reader[7]),
                            id = Convert.ToDouble(reader[8] == DBNull.Value ? 0 : reader[8]),
                            grandTotal = Convert.ToDouble(reader[9] == DBNull.Value ? 0 : reader[9]),
                        });
                    }
                }
                else
                {
                }
                reader.Close();


            }
            connection.Close();

            return list;

        }

        
        [HttpGet]
        public List<DepartmentViewModel> GetDepartment()
        {
            List<DepartmentViewModel> list = new List<DepartmentViewModel>();

            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@"select 
                                                id ,
                                                description 
                                                from  department pd
                                                where id != 1", connection))
            {
                cmd.Prepare();

                NpgsqlDataReader reader = cmd.ExecuteReader();


                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        list.Add(new DepartmentViewModel
                        {
                            id = Convert.ToInt16(reader[0]),
                            description = reader[1].ToString(),
                        });
                    }
                }
                else
                {
                }
                reader.Close();


            }
            connection.Close();

            return list;

        }

        
        [HttpGet]
        public List<CategoryViewModel> GetCategory()
        {
            List<CategoryViewModel> list = new List<CategoryViewModel>();

            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@"select 
                                                id ,
                                                description 
                                                from  category pd where id != 1", connection))
            {
                cmd.Prepare();

                NpgsqlDataReader reader = cmd.ExecuteReader();


                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        list.Add(new CategoryViewModel
                        {
                            id = Convert.ToInt16(reader[0]),
                            description = reader[1].ToString(),
                        });
                    }
                }
                else
                {
                }
                reader.Close();


            }
            connection.Close();

            return list;

        }


        [HttpGet]
        public List<StoreViewModel> GetStore()
        {
            List<StoreViewModel> list = new List<StoreViewModel>();

            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@"select 
                                                id ,
                                                description 
                                                from  store ps ", connection))
            {
                cmd.Prepare();

                NpgsqlDataReader reader = cmd.ExecuteReader();


                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        list.Add(new StoreViewModel
                        {
                            id = Convert.ToInt16(reader[0]),
                            description = reader[1].ToString(),
                        });
                    }
                }
                else
                {
                }
                reader.Close();


            }
            connection.Close();

            return list;

        }

        [HttpGet]
        public List<WeeklyReportViewModel> GetWeeklyReport(double? storeId)
        {
            List<WeeklyReportViewModel> list = new List<WeeklyReportViewModel>();

            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@"select distinct 
                                                    pwr.itemid ,
                                                    vp.title ,
                                                    wrl.itemcount ,
                                                    vp.image
                                                    from  weekly_report  pwr
                                                    join vw_product vp on vp.id = pwr.itemid
                                                    left join  weekly_report_log wrl on wrl.itemid = pwr.itemid 
                                                    where  isselected = true
                                                    and instock is not null
                                                    and case when :storeId = 0  then pwr.storeid  is not null else pwr.storeid  = :storeId::numeric end ", connection))
            {
                cmd.Parameters.AddWithValue("storeId", storeId == 0 ? DBNull.Value : storeId);
                NpgsqlDataReader reader = cmd.ExecuteReader();


                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        list.Add(new WeeklyReportViewModel
                        {
                            id = reader[0].ToString(),
                            name = reader[1].ToString(),
                            count = Convert.ToDouble(reader[2] == DBNull.Value ? 0 : reader[2]),
                            image = reader[3].ToString(),
                        });
                    }
                }
                else
                {
                }
                reader.Close();


            }
            connection.Close();

            return list;

        }


        //[HttpGet]
        //public List<MessageViewModel> GetMessages(string? fromStore, string? toStore)
        //{
        //    List<MessageViewModel> list = new List<MessageViewModel>();

        //    NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
        //    connection.Open();

        //    using (var cmd = new NpgsqlCommand(@"select
        //                                            id,
        //                                            fromstore || ': ' ||message 
        //                                            from  message pm  
        //                                            where fromstore = :fromStore and tostore = :toStore order by id desc ", connection))
        //    {
        //        cmd.Parameters.AddWithValue("fromStore", fromStore == null ? DBNull.Value : fromStore);
        //        cmd.Parameters.AddWithValue("toStore", toStore == null ? DBNull.Value : toStore);
        //        cmd.Prepare();
        //        NpgsqlDataReader reader = cmd.ExecuteReader();


        //        if (reader.HasRows)
        //        {
        //            while (reader.Read())
        //            {
        //                list.Add(new MessageViewModel
        //                {
        //                    id = Convert.ToDouble(reader[0]),
        //                    message = reader[1].ToString(),
        //                });
        //            }
        //        }
        //        else
        //        {
        //        }
        //        reader.Close();


        //    }
        //    connection.Close();

        //    return list;

        //}


        [HttpGet]
        public List<CashBoxViewModel> GetCashBox(double storeId)
        {
            List<CashBoxViewModel> cashBoxViewModel = new List<CashBoxViewModel>();

            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@"select 
                                                cashbox ,
                                                envelope ,
                                                sum(grandtotal)/count(distinct invoiceid) as dailyearnings
                                                from  cash_box pcb 
                                                right join invoice i on i.storeid = pcb.storeid 
                                                where paymentmethod = 1 and i.storeid = :storeId and date(i.createdon) = current_date  group by  date(i.createdon),cashbox ,envelope ", connection))
            {
                cmd.Parameters.AddWithValue("storeId", storeId == null ? DBNull.Value : storeId);
                cmd.Prepare();
                NpgsqlDataReader reader = cmd.ExecuteReader();


                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        cashBoxViewModel.Add(new CashBoxViewModel
                        {
                            cashbox = Convert.ToDouble(reader[0] == DBNull.Value ? 0 : reader[0]),
                            envelope = Convert.ToDouble(reader[1] == DBNull.Value ? 0 : reader[1]),
                            dailyEarnings = Math.Round(Convert.ToDouble(reader[2] == DBNull.Value ? 0 : reader[2]),2),
                        });
                    }
                }
                else
                {
                }
                reader.Close();


            }
            connection.Close();

            return cashBoxViewModel;

        }



        [HttpGet]
        public dynamic InvoiceDetailList(double? invPerId = null)
        {
            List<ReportInvoiceDetailViewModel> reportInvoiceDetail = new List<ReportInvoiceDetailViewModel>();
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@"select 
                                                    invoiceid ,
                                                    productid ,
                                                    productname ,
                                                    quantity ,
                                                    pp.personnelname  ,
                                                    ps.description  ,
                                                    pp2.description ,
                                                    sellprice ,
                                                    subtotal ,
                                                    taxtotal ,
                                                    grandtotal ,
                                                    customerphone ,
                                                    i.createdon
                                                    from invoice i 
                                                    join  personnel pp on pp.personnelcode = i.personnelid 
                                                    join  store ps on ps.id = i.storeid 
                                                    join  paymentmethod pp2 on pp2.id = i.paymentmethod 
                                                    where invoiceid = :invPerId or customerphone = :invPerId::text", connection))
            {
                cmd.Parameters.AddWithValue("invPerId", invPerId == null ? 0 : invPerId);
                cmd.Prepare();
                NpgsqlDataReader reader = cmd.ExecuteReader();


                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        reportInvoiceDetail.Add(new ReportInvoiceDetailViewModel
                        {
                            invoiceId = Convert.ToDouble(reader[0] == DBNull.Value ? null : reader[0]),
                            productId = Convert.ToDouble(reader[1] == DBNull.Value ? null : reader[1]),
                            productName = reader[2].ToString() == DBNull.Value.ToString() ? null : reader[2].ToString(),
                            quantity = Convert.ToDouble(reader[3] == DBNull.Value ? null : reader[3]),
                            personnelName = reader[4].ToString() == DBNull.Value.ToString() ? null : reader[4].ToString(),
                            storeName = reader[5].ToString() == DBNull.Value.ToString() ? null : reader[5].ToString(),
                            paymentMethod = reader[6].ToString() == DBNull.Value.ToString() ? null : reader[6].ToString(),
                            sellPrice = Convert.ToDouble(reader[7] == DBNull.Value ? null : reader[7]),
                            subTotal = Convert.ToDouble(reader[8] == DBNull.Value ? null : reader[8]),
                            taxTotal = Convert.ToDouble(reader[9] == DBNull.Value ? null : reader[9]),
                            grandTotal = Convert.ToDouble(reader[10] == DBNull.Value ? null : reader[10]),
                            customerPhone = Convert.ToDouble(reader[11] == DBNull.Value ? null : reader[11]),
                            getDate = Convert.ToDateTime(reader[12] == DBNull.Value ? null : reader[12]),
                        });
                    }
                }
                else
                {
                }
                reader.Close();


            }
            connection.Close();

            return reportInvoiceDetail;

        }


        [HttpPost]
        public dynamic ChangeCashTradeIn(double storeId)
        {

            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@$"
                                                update sales 
                                                set
                                                paymentoldsellprice = sellprice ,
                                                sellprice = sellprice - (sellprice *85/100)
                                                where sellprice < 0 and storeid = :storeId", connection))
            {
                cmd.Parameters.AddWithValue("storeId", storeId);
                cmd.ExecuteNonQuery();

            }
            connection.Close();





            return true;
        }

        [HttpPost]
        public dynamic ChangeReCashTradeIn(double storeId)
        {

            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@$"update sales 
                                                    set
                                                    sellprice =  case when paymentoldsellprice is null then sellprice else paymentoldsellprice end + case when paymentoldsellprice is null then oldsellprice else paymentoldsellprice end *85/100
                                                    where sellprice < 0 and storeid = :storeId", connection))
            {
                cmd.Parameters.AddWithValue("storeId", storeId);
                cmd.ExecuteNonQuery();

            }
            connection.Close();





            return true;
        }

        [HttpPost]
        public dynamic ChangeCreditCardTradeIn(double storeId)
        {

            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@$"update sales 
                                                set
                                                sellprice = case when paymentoldsellprice is null then sellprice else paymentoldsellprice end
                                                where sellprice < 0 and storeid = :storeId", connection))
            {
                cmd.Parameters.AddWithValue("storeId", storeId);
                cmd.ExecuteNonQuery();

            }
            connection.Close();





            return true;
        }


        [HttpPost]
        public dynamic ChangeGiftCodeTradeIn(double storeId)
        {

            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@$"
                                                    update sales 
                                                    set
                                                    paymentoldsellprice = sellprice ,
                                                    sellprice = sellprice - (sellprice *80/100)
                                                    where sellprice < 0 and storeid = :storeId", connection))
            {
                cmd.Parameters.AddWithValue("storeId", storeId);
                cmd.ExecuteNonQuery();

            }
            connection.Close();





            return true;
        }


        [HttpPost]
        public dynamic ChangeReGiftCodeTradeIn(double storeId)
        {

            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@$"update sales 
                                                    set
                                                    sellprice = case when paymentoldsellprice is null then sellprice else paymentoldsellprice end + case when paymentoldsellprice is null then oldsellprice else paymentoldsellprice end *80/100
                                                    where sellprice < 0 and storeid = :storeId", connection))
            {
                cmd.Parameters.AddWithValue("storeId", storeId);
                cmd.ExecuteNonQuery();

            }
            connection.Close();





            return true;
        }


        [HttpPost]
        public dynamic UpdateCashBox(double storeId, string personnelCode,double cashBox,double envelope )
        {

            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@$"insert into  cash_box (storeid,personnelcode,cashbox,envelope,createdon)
                                            values(:storeId,:personnelCode,:cashBox,:envelope,current_timestamp)", connection))
            {
                cmd.Parameters.AddWithValue("storeId", storeId);
                cmd.Parameters.AddWithValue("personnelCode", personnelCode);
                cmd.Parameters.AddWithValue("cashBox", cashBox);
                cmd.Parameters.AddWithValue("envelope", envelope);
                cmd.ExecuteNonQuery();

            }
            connection.Close();





            return true;
        }


        [HttpPost]
        public dynamic UpdatePrice(double price, double itemId)
        {

            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@$"update  product c
                                                    set price = {price}
                                                    where itemid = {itemId}::text ", connection))
            {
                cmd.ExecuteNonQuery();

            }
            connection.Close();





            return true;
        }


        [HttpPost]
        public dynamic UpdateProductQuality(double itemId, double qualityRate)
        {

            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@$"update sales 
                                                            set
                                                            discount = :discount, 
                                                            sellprice = sellprice + (case when oldsellprice is null then sellprice else oldsellprice end * :discount /100)
                                                            where 
                                                            id = :id", connection))
            {
                cmd.Parameters.AddWithValue("id", itemId);
                cmd.Parameters.AddWithValue("discount", qualityRate);
                cmd.ExecuteNonQuery();

            }
            connection.Close();





            return true;
        }


        [HttpPost]
        public dynamic RepairToStock(string itemId, double storeId)
        {

            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@$"  UPDATE  product 
                                           SET 
                                           instock = case when  product.instock is null then 1 else  product.instock + 1 end,
                                           inrepair = ( product.inrepair)-1,
                                           updatedon = current_timestamp 
                                        WHERE itemid  = :itemId and storeid = :storeId 	", connection))
            {
                cmd.Parameters.AddWithValue("itemid", itemId);
                cmd.Parameters.AddWithValue("storeId", storeId);
                cmd.ExecuteNonQuery();

            }
            connection.Close();





            return true;
        }


        [HttpPost]
        public dynamic CreateProductRequest(double itemId,string itemName, double storeId,  double sellPrice, string cashierId)
        {

            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@$" INSERT INTO  product (itemid, itemname, instock , storeid,sellprice, createdon, updatedon)
                                              values(:itemId ,
                                              :itemName,
                                              -1,
                                              :storeId,
                                              :sellPrice,
                                              current_timestamp,
                                              current_timestamp)
                                            ON CONFLICT(itemid, storeid)
                                            DO UPDATE 
                                            SET
  	                                              instock = case when public. product.instock is null then -1 else public. product.instock  - 1 end,
  	                                              updatedon = current_timestamp	", connection))
            {
                cmd.Parameters.AddWithValue("itemid", itemId);
                cmd.Parameters.AddWithValue("itemName", itemName);
                cmd.Parameters.AddWithValue("storeId", storeId);
                cmd.Parameters.AddWithValue("sellPrice", sellPrice);
                cmd.ExecuteNonQuery();
            }

            using (var cmd = new NpgsqlCommand(@$"insert into  product_request(itemid,storeid,personnelcode ,createdon)
                                                    values(:itemId,:storeId,:personnelCode,current_timestamp)", connection))
            {
                cmd.Parameters.AddWithValue("itemid", itemId);
                cmd.Parameters.AddWithValue("storeId", storeId);
                cmd.Parameters.AddWithValue("personnelCode", cashierId);
                cmd.ExecuteNonQuery();
            }
            connection.Close();





            return true;
        }


        [HttpPost]
        public dynamic ProductTradeIn(double itemId, string itemName, bool tradeIn, double discount, double buyPrice, double storeId)
        {
            //log table
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@" insert into trade_in_product (itemid, itemname, discount, buyprice,storeid, createdon) 
                                                values (:itemId,:itemName, :discount,:buyPrice,:storeId,current_timestamp)	", connection))
            {
                cmd.Parameters.AddWithValue("itemid", itemId);
                cmd.Parameters.AddWithValue("itemName", itemName);
                cmd.Parameters.AddWithValue("discount", discount);
                cmd.Parameters.AddWithValue("buyPrice", buyPrice);
                cmd.Parameters.AddWithValue("storeId", storeId);
                cmd.ExecuteNonQuery();

            }

            var query = "";
            if (tradeIn == true)
            {
                query = @"INSERT INTO  product (itemid, itemname, inrepair, storeid, createdon, updatedon)
                                              values(:itemId ,
                                              :itemName,
                                              1,
                                              :storeId,
                                              current_timestamp,
                                              current_timestamp)
                                            ON CONFLICT(itemid, storeid)
                                            DO UPDATE 
                                            SET
  	                                              inrepair = case when public. product.inrepair is null then 1 else public. product.inrepair  + 1 end,
  	                                              updatedon = current_timestamp";
            }
            else
            {
                query = @"INSERT INTO  product (itemid, itemname, instock, storeid, createdon, updatedon)
                                              values(:itemId ,
                                              :itemName,
                                              1,
                                              :storeId,
                                              current_timestamp,
                                              current_timestamp)
                                            ON CONFLICT(itemid, storeid)
                                            DO UPDATE 
                                            SET
  	                                              instock = case when public. product.instock is null then 1 else public. product.instock + 1 end,
  	                                              updatedon = current_timestamp";
            }

            using (var cmd = new NpgsqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("itemId", itemId);
                cmd.Parameters.AddWithValue("itemName", itemName);
                cmd.Parameters.AddWithValue("storeId", storeId);
                cmd.ExecuteNonQuery();

            }
            connection.Close();





            return true;
        }

        
        [HttpPost]
        public dynamic AddSale(string itemId, string itemName, double sellPrice, double storeId)
        {
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

           


            using (var cmd = new NpgsqlCommand(@"insert into  sales (itemid, itemname , quantity, sellprice,storeid, createdon) 
                                                values (:itemId,:itemName, 1,:sellPrice,:storeId,current_timestamp)	", connection))
            {
                cmd.Parameters.AddWithValue("itemid", itemId);
                cmd.Parameters.AddWithValue("itemName", itemName);
                cmd.Parameters.AddWithValue("sellPrice", Math.Round(sellPrice, 2));
                cmd.Parameters.AddWithValue("storeId", storeId);
                cmd.ExecuteNonQuery();

            }

            using (var cmd = new NpgsqlCommand(@"insert into sales_info (itemid, itemname,  quantity, sellprice,storeid, createdon) 
                                                values (:itemId,:itemName, 1,:sellPrice,:storeId,current_timestamp)	", connection))
            {
                cmd.Parameters.AddWithValue("itemid", itemId);
                cmd.Parameters.AddWithValue("itemName", itemName);
                cmd.Parameters.AddWithValue("sellPrice", sellPrice);
                cmd.Parameters.AddWithValue("storeId", storeId);
                cmd.ExecuteNonQuery();

            }



            using (var cmd = new NpgsqlCommand(@"update  product
                                set
                                instock = product.instock -1
                                where
                                storeid = :storeId and itemid = :itemid ", connection))
            {
                cmd.Parameters.AddWithValue("itemid", itemId);
                cmd.Parameters.AddWithValue("storeId", storeId);
                cmd.ExecuteNonQuery();
            }


            //var query = "";
            //if(tradeIn == true && toRepair == true)
            //{
            //    query = @"INSERT INTO  product (itemid, itemname, inrepair, storeid,sellprice, createdon, updatedon)
            //                                  values(:itemId ,
            //                                  :itemName,
            //                                  :quantity,
            //                                  :storeId,
            //                                  :sellPrice,
            //                                  current_timestamp,
            //                                  current_timestamp)
            //                                ON CONFLICT(itemid, storeid)
            //                                DO UPDATE 
            //                                SET
            //                                     inrepair = case when public. product.inrepair is null then :quantity else public. product.inrepair  + :quantity end,
            //                                     updatedon = current_timestamp";
            //}
            //else
            //{
            //    query = @"INSERT INTO  product (itemid, itemname, instock, storeid,sellprice, createdon, updatedon)
            //                                  values(:itemId ,
            //                                  :itemName,
            //                                  -:quantity,
            //                                  :storeId,
            //                                  :sellPrice,
            //                                  current_timestamp,
            //                                  current_timestamp)
            //                                ON CONFLICT(itemid, storeid)
            //                                DO UPDATE 
            //                                SET
            //                                     instock = case when public. product.instock is null then -:quantity else public. product.instock - :quantity end,
            //                                     updatedon = current_timestamp ";
            //}




            //using (var cmd = new NpgsqlCommand(query, connection))
            //{
            //    cmd.Parameters.AddWithValue("itemid", itemId);
            //    cmd.Parameters.AddWithValue("itemName", itemName);
            //    cmd.Parameters.AddWithValue("quantity", quantity);
            //    cmd.Parameters.AddWithValue("storeId", storeId);
            //    cmd.Parameters.AddWithValue("sellPrice", sellPrice);
            //    cmd.ExecuteNonQuery();
            //}
            //connection.Close();





            return true;
        }

        
        [HttpPost]
        public dynamic AddProduct(string? itemId, string itemName, double departmentId, double categoryId, double stock, double sellPrice, double storeId)
        {
            //log table
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();
            var query = "";
            var productQuery = "";
            var barcodeQuery = "";
            if (itemId == null || itemId == "0")
            {
                query = @"insert into generate_product (description,createdon) 
                                                    values(:itemName, current_timestamp)
                                                    ON CONFLICT(description) DO UPDATE set updatedon = current_timestamp";
                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("itemName", itemName);
                    cmd.Parameters.AddWithValue("storeId", storeId);
                    cmd.ExecuteNonQuery();

                }

                productQuery = @" insert into  product  (itemid ,itemname , departmentid , categoryid , instock ,sellprice ,storeid , createdon)
                        select max(id) , :itemName , :departmentId , :categoryId , :inStock ,:sellPrice, :storeId , current_timestamp from generate_product limit 1";

                using (var cmd = new NpgsqlCommand(productQuery, connection))
                {
                    cmd.Parameters.AddWithValue("itemName", itemName);
                    cmd.Parameters.AddWithValue("departmentId", departmentId);
                    cmd.Parameters.AddWithValue("categoryId", categoryId);
                    cmd.Parameters.AddWithValue("inStock", stock);
                    cmd.Parameters.AddWithValue("sellPrice", Math.Round(sellPrice, 2));
                    cmd.Parameters.AddWithValue("storeId", storeId);
                    cmd.ExecuteNonQuery();
                }


                barcodeQuery = @"insert into  product_barcode (itemid,itemname,sellprice,storeid,itemungenerated,createdon,updatedon)
                                values((select max(id) from generate_product limit 1), :itemName , :sellPrice, :storeId ,:inStock, current_timestamp,current_timestamp )";

                using (var cmd = new NpgsqlCommand(barcodeQuery, connection))
                {
                    cmd.Parameters.AddWithValue("itemName", itemName);
                    cmd.Parameters.AddWithValue("sellPrice", Math.Round(sellPrice, 2));
                    cmd.Parameters.AddWithValue("storeId", storeId);
                    cmd.Parameters.AddWithValue("inStock", stock);
                    cmd.ExecuteNonQuery();
                }


            }
            else
            {
                productQuery = @$" insert into  product  (itemid ,itemname , departmentid , categoryid , instock ,sellprice ,storeid , createdon)
                        values(  :itemId, :itemName , :departmentId , :categoryId , :inStock ,:sellPrice, :storeId , current_timestamp) 
                        ON CONFLICT (itemid,storeid) 
                        DO UPDATE SET
                                                  itemname = :itemName,
                                                  departmentid = :departmentId, 
  	                                              categoryid = :categoryId, 
  	                                              instock = :inStock, 
  	                                              sellprice = :sellPrice,
  	                                              updatedon = current_timestamp";
                using (var cmd = new NpgsqlCommand(productQuery, connection))
                {
                    cmd.Parameters.AddWithValue("itemId", itemId);
                    cmd.Parameters.AddWithValue("itemName", itemName);
                    cmd.Parameters.AddWithValue("departmentId", departmentId);
                    cmd.Parameters.AddWithValue("categoryId", categoryId);
                    cmd.Parameters.AddWithValue("inStock", stock);
                    cmd.Parameters.AddWithValue("sellPrice", Math.Round(sellPrice, 2));
                    cmd.Parameters.AddWithValue("storeId", storeId);
                    cmd.ExecuteNonQuery();
                }                

                barcodeQuery = @$" insert into  product_barcode  (itemid ,itemname ,sellprice ,storeid,itemungenerated , createdon,updatedon)
                        values(  :itemId, :itemName ,:sellPrice, :storeId ,:inStock , current_timestamp,current_timestamp) 
                        ON CONFLICT (itemid,storeid) 
                        DO UPDATE SET
                                                  itemname = :itemName,
                                                  itemungenerated  =  product_barcode.itemungenerated + :inStock ,
  	                                              sellprice = :sellPrice,
  	                                              updatedon = current_timestamp";
                using (var cmd = new NpgsqlCommand(barcodeQuery, connection))
                {
                    cmd.Parameters.AddWithValue("itemId", itemId);
                    cmd.Parameters.AddWithValue("itemName", itemName);
                    cmd.Parameters.AddWithValue("inStock", stock);
                    cmd.Parameters.AddWithValue("sellPrice", Math.Round(sellPrice, 2));
                    cmd.Parameters.AddWithValue("storeId", storeId);
                    cmd.ExecuteNonQuery();
                }
            }
           
            connection.Close();
            return true;
        }

        
        [HttpPost]
        public dynamic DeleteProduct(string itemId,  double storeId)
        {
            //log table
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

          

            using (var cmd = new NpgsqlCommand(@" delete from  product pp where itemid = :itemId and storeid = :storeid ", connection))
            {
                cmd.Parameters.AddWithValue("itemId", itemId);
                cmd.Parameters.AddWithValue("storeId", storeId);
                cmd.ExecuteNonQuery();
            }
            connection.Close();
            return true;
        }

        
        [HttpPost]
        public dynamic DeleteSaleInfo(double itemId, double storeId , double quantity, bool tradeIn , bool toRepair)
        {
            //log table
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();
            var query = "";
           

            using (var cmd = new NpgsqlCommand(@" delete from sales gp where itemid = :itemId and storeid = :storeId", connection))
            {
                cmd.Parameters.AddWithValue("itemId", itemId);
                cmd.Parameters.AddWithValue("storeId", storeId);
                cmd.ExecuteNonQuery();

            }


            using (var cmd = new NpgsqlCommand(@" delete from sales_info gp where itemid = :itemId and storeid = :storeId", connection))
            {
                cmd.Parameters.AddWithValue("itemId", itemId);
                cmd.Parameters.AddWithValue("storeId", storeId);
                cmd.Parameters.AddWithValue("quantity", quantity);
                cmd.Parameters.AddWithValue("tradeIn", tradeIn == null ? false : tradeIn);
                cmd.ExecuteNonQuery();

            }

            if (tradeIn == true && toRepair == true)
            {
                query = @"update  product 
                                set
                                inrepair  =  inrepair -:quantity
                                where
                                storeid = :storeId and itemid = :itemid ";
            }
            else if (tradeIn == false && toRepair == false)
            {
                query = @"update  product 
                                set
                                instock  =  instock + :quantity
                                where
                                storeid = :storeId and itemid = :itemid ";
            }
            else
            {
                query = @"update  product 
                                set
                                instock  =  instock -:quantity
                                where
                                storeid = :storeId and itemid = :itemid  ";
            }
            using (var cmd = new NpgsqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("itemid", itemId);
                cmd.Parameters.AddWithValue("quantity", quantity);
                cmd.Parameters.AddWithValue("storeId", storeId);
                cmd.ExecuteNonQuery();
            }
            connection.Close();
            return true;
        }

        
        [HttpPost]
        public dynamic Invoice(string authPayload ,double paymentMethod, double storeId, string? customerPhone, string? giftCode = null)
        {
            //log table

            TwilioClient.Init("AC2c5b50cdd058de5285f255a906991ed2", "e74dbc38dbf12287d264a2e1e122df2c");
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();
            var response = "fail";
            var gifted = "";

            //TOTP ID verification
                    var challenge = ChallengeResource.Create(
                   authPayload: authPayload,
                   factorSid: "YF02633f48adfd491d563cb36f4acc0aa0",
                   pathServiceSid: "VAae5efa633236d7c0597ae60f35c56ac9",
                   pathIdentity: "ff483d1ff591898a9942916050d2ca3f"
                    );
                    if (response == "fail")
                    {
                        response = (challenge.Status.ToString() == "pending" ? "fail" : "success");
                    }
                    else
                    {

                        using (var cmd = new NpgsqlCommand(@"select personnelcode  
                                                            from  personnel pp 
                                                            left join  lkp_verifycode plv on plv.id = pp.verifyid 
                                                            where storeid = :storeId and plv.factorsid = 'YF02633f48adfd491d563cb36f4acc0aa0'", connection))
                        {
                            cmd.Parameters.AddWithValue("storeId", storeId);
                            cmd.Prepare();
                            NpgsqlDataReader reader = cmd.ExecuteReader();


                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    personnelCode = reader[0].ToString();
                                };
                            }
                            else
                            {
                            }
                            reader.Close();


                        }
                    };
                    var secondchallenge = ChallengeResource.Create(
                   authPayload: authPayload,
                        factorSid: "YF02633cd7073f5945ede24b4366ffa87c",
                        pathServiceSid: "VAae5efa633236d7c0597ae60f35c56ac9",
                        pathIdentity: "ff483d1ff591898a9942916050d2ca3f"
                    );
                    if (response == "fail")
                    {
                        response = (secondchallenge.Status.ToString() == "pending" ? "fail" : "success");
                    }
                    else
                    {

                        using (var cmd = new NpgsqlCommand(@"select personnelcode  
                                                            from  personnel pp 
                                                            left join  lkp_verifycode plv on plv.id = pp.verifyid 
                                                            where storeid = :storeId and plv.factorsid = 'YF02633cd7073f5945ede24b4366ffa87c'", connection))
                        {
                            cmd.Parameters.AddWithValue("storeId", storeId);
                            cmd.Prepare();
                            NpgsqlDataReader reader = cmd.ExecuteReader();


                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    personnelCode = reader[0].ToString();
                                };
                            }
                            else
                            {
                            }
                            reader.Close();


                        }
                    };
                    var thirdchallenge = ChallengeResource.Create(
                        authPayload: authPayload,
                        factorSid: "YF02634026215262c2da050d7c9e5471da",
                        pathServiceSid: "VAae5efa633236d7c0597ae60f35c56ac9",
                        pathIdentity: "ff483d1ff591898a9942916050d2ca3f"
                    );
                    if (response == "fail")
                    {
                        response = (thirdchallenge.Status.ToString() == "pending" ? "fail" : "success");
                    }
                    else
                    {

                        using (var cmd = new NpgsqlCommand(@"select personnelcode  
                                                            from  personnel pp 
                                                            left join  lkp_verifycode plv on plv.id = pp.verifyid 
                                                            where storeid = :storeId and plv.factorsid = 'YF02634026215262c2da050d7c9e5471da'", connection))
                        {
                            cmd.Parameters.AddWithValue("storeId", storeId);
                            cmd.Prepare();
                            NpgsqlDataReader reader = cmd.ExecuteReader();


                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    personnelCode = reader[0].ToString();
                                };
                            }
                            else
                            {
                            }
                            reader.Close();


                        }
                    };
                    var fourthchallenge = ChallengeResource.Create(
                        authPayload: authPayload,
                        factorSid: "YF026340270b970dc8e834c4e3d127168f",
                        pathServiceSid: "VAae5efa633236d7c0597ae60f35c56ac9",
                        pathIdentity: "ff483d1ff591898a9942916050d2ca3f"
                    );
                    if (response == "fail")
                    {
                        response = (fourthchallenge.Status.ToString() == "pending" ? "fail" : "success");
                    }
                    else
                    {

                        using (var cmd = new NpgsqlCommand(@"select personnelcode  
                                                            from  personnel pp 
                                                            left join  lkp_verifycode plv on plv.id = pp.verifyid 
                                                            where storeid = :storeId and plv.factorsid = 'YF026340270b970dc8e834c4e3d127168f'", connection))
                        {
                            cmd.Parameters.AddWithValue("storeId", storeId);
                            cmd.Prepare();
                            NpgsqlDataReader reader = cmd.ExecuteReader();


                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    personnelCode = reader[0].ToString();
                                };
                            }
                            else
                            {
                            }
                            reader.Close();


                        }
                    };
                    var fifthchallenge = ChallengeResource.Create(
                        authPayload: authPayload,
                        factorSid: "YF02634042148ed5621b01c63341229ee0",
                        pathServiceSid: "VAae5efa633236d7c0597ae60f35c56ac9",
                        pathIdentity: "ff483d1ff591898a9942916050d2ca3f"
                    );
                    if (response == "fail")
                    {
                        response = (fifthchallenge.Status.ToString() == "pending" ? "fail" : "success");
                    }
                    else
                    {

                        using (var cmd = new NpgsqlCommand(@"select personnelcode  
                                                            from  personnel pp 
                                                            left join  lkp_verifycode plv on plv.id = pp.verifyid 
                                                            where storeid = :storeId and plv.factorsid = 'YF02634042148ed5621b01c63341229ee0'", connection))
                        {
                            cmd.Parameters.AddWithValue("storeId", storeId);
                            cmd.Prepare();
                            NpgsqlDataReader reader = cmd.ExecuteReader();


                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    personnelCode = reader[0].ToString();
                                };
                            }
                            else
                            {
                            }
                            reader.Close();


                        }
                    };
                    var sixthchallenge = ChallengeResource.Create(
                        authPayload: authPayload,
                        factorSid: "YF026340434079ce5d493973c90ecc86ee",
                        pathServiceSid: "VAae5efa633236d7c0597ae60f35c56ac9",
                        pathIdentity: "ff483d1ff591898a9942916050d2ca3f"
                    );
                    if (response == "fail")
                    {
                        response = (sixthchallenge.Status.ToString() == "pending" ? "fail" : "success");
                    }
                    else
                    {

                        using (var cmd = new NpgsqlCommand(@"select personnelcode  
                                                            from  personnel pp 
                                                            left join  lkp_verifycode plv on plv.id = pp.verifyid 
                                                            where storeid = :storeId and plv.factorsid = 'YF026340434079ce5d493973c90ecc86ee'", connection))
                        {
                            cmd.Parameters.AddWithValue("storeId", storeId);
                            cmd.Prepare();
                            NpgsqlDataReader reader = cmd.ExecuteReader();


                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    personnelCode = reader[0].ToString();
                                };
                            }
                            else
                            {
                            }
                            reader.Close();


                        }
                    };

            if (response != "fail")
            {
                //Generate New Invoice Id
                using (var cmd = new NpgsqlCommand(@"insert into invoice_id (storeid,createdon) values (:storeId,current_timestamp) ", connection))
                {
                    cmd.Parameters.AddWithValue("storeId", storeId);
                    cmd.ExecuteNonQuery();
                }

                //Select Gift If Exist
                using (var cmd = new NpgsqlCommand(@"select invoiceid from  giftcode where giftcode =:giftCode", connection))
                {
                    cmd.Parameters.AddWithValue("giftCode", giftCode == null ? "" : giftCode);
                    gifted = cmd.ExecuteScalar()?.ToString();
                }
                if (gifted != null)
                {
                    //Temporary Sales Table Is Transferred To Invoice Table
                    using (var cmd = new NpgsqlCommand(@"with data as (select storeid ,
                                                case 
                                                when sum(sellprice)  > 49 and 100 > sum(sellprice)  then sum(sellprice)-5  
                                                when sum(sellprice)  > 99 and 200 > sum(sellprice) then sum(sellprice)-10 
                                                when sum(sellprice)  > 200 then sum(sellprice)-20 
                                                else sum(sellprice)
                                                end as subtotal, 
                                                case 
                                                when sum(sellprice)  > 49 and 100 > sum(sellprice)  then (sum(sellprice)-5)/ps.taxrate   
                                                when sum(sellprice)  > 99 and 200 > sum(sellprice) then (sum(sellprice)-10)/ps.taxrate  
                                                when sum(sellprice)  > 200 then (sum(sellprice)-20)/ps.taxrate  
                                                else sum(sellprice)
                                                end as taxtotal, 
                                                case 
                                                when sum(sellprice)  > 49 and 100 > sum(sellprice)  then sum(sellprice)-5+(sum(sellprice)-5)/ps.taxrate   
                                                when sum(sellprice)  > 99 and 200 > sum(sellprice) then sum(sellprice)-10+(sum(sellprice)-10)/ps.taxrate 
                                                when sum(sellprice)  > 200 then sum(sellprice)-20+(sum(sellprice)-20)/ps.taxrate 
                                                else sum(sellprice)
                                                end as grandtotal
                                                from sales s
                                                join  store ps on ps.id = s.storeid  group by storeid,ps.taxrate) 
                                                insert into invoice ( invoiceId,
                                                productid,
                                                productname,	
                                                quantity, 	
                                                personnelId, 	
                                                storeid, 	
                                                sellprice, 	
                                                paymentMethod,
                                                subtotal, 	
                                                taxtotal, 	
                                                grandtotal, 	
                                                createdon,
                                                customerphone
                                                )
                                                select (select max(id) from invoice_id),itemId , itemname ,quantity ,:personnelId,pp.storeid,sellprice , :paymentMethod, data.subtotal, data.taxtotal , data.grandtotal, current_timestamp, :customerPhone  from sales pp
                                                join data on data.storeid = pp.storeid 
                                                where pp.storeid = :storeid ", connection))
                    {
                        cmd.Parameters.AddWithValue("personnelId", personnelCode);
                        cmd.Parameters.AddWithValue("paymentMethod", paymentMethod);
                        cmd.Parameters.AddWithValue("storeId", storeId);
                        cmd.Parameters.AddWithValue("customerPhone", customerPhone == null ? DBNull.Value : customerPhone.Replace("+", "").Trim().ToString());
                        cmd.ExecuteNonQuery();

                    }
                }
                else {
                    using (var cmd = new NpgsqlCommand(@"with data as (with salesdata as(select storeid ,sum(sellprice) as subtotal,  sum(sellprice)/ps.taxrate  as taxtotal , sum(sellprice)+ sum(sellprice)/ps.taxrate  as grandtotal 
                                                                                from sales s
                                                                                join  store ps on ps.id= s.storeid 
                                                                                where sellprice > 0
                                                                                group by storeid,ps.taxrate
                                                union                                                                               
                                                select storeid ,sum(sellprice) as subtotal, 0  as taxtotal , sum(sellprice)  as grandtotal 
                                                                                from sales s
                                                                                join  store ps on ps.id= s.storeid 
                                                                                where sellprice < 0
                                                                                group by storeid,ps.taxrate)                                              
                                                select storeid, sum(subtotal) as subtotal, sum(taxtotal) as taxtotal, sum(grandtotal) as grandtotal from salesdata  
                                                where storeid =:storeId
                                                group by storeid) 
                                                insert into invoice ( invoiceId,
                                                productid,
                                                productname,	
                                                quantity, 	
                                                personnelId, 	
                                                storeid, 	
                                                sellprice, 	
                                                paymentMethod,
                                                subtotal, 	
                                                taxtotal, 	
                                                grandtotal, 	
                                                createdon,
                                                customerphone
                                                )
                                                select (select max(id) from invoice_id),itemId , itemname ,quantity ,:personnelId,pp.storeid,sellprice , :paymentMethod, data.subtotal, data.taxtotal , data.grandtotal, current_timestamp, :customerPhone  from sales pp
                                                join data on data.storeid = pp.storeid 
                                                where pp.storeid = :storeid ", connection))
                    {
                        cmd.Parameters.AddWithValue("personnelId", personnelCode);
                        cmd.Parameters.AddWithValue("paymentMethod", paymentMethod);
                        cmd.Parameters.AddWithValue("storeId", storeId);
                        cmd.Parameters.AddWithValue("customerPhone", customerPhone == null ? DBNull.Value : customerPhone.Replace("+", "").Trim().ToString());
                        cmd.ExecuteNonQuery();

                    }
                }
            
                //Generate New Gift Code
                using (var cmd = new NpgsqlCommand(@"insert into  giftcode (invoiceid, customerphone ,giftcode ,isused ,createdon) 
                                                        select (select max(id) from invoice_id), :customerPhone ,make_uid(),false, current_timestamp 
                                                        from  promotion pc
                                                        where pc.startdate <= current_timestamp and current_timestamp <= pc.enddate limit 1", connection))
                {
                    cmd.Parameters.AddWithValue("customerPhone",customerPhone == null ? DBNull.Value : customerPhone?.Replace("+", "").Trim().ToString());
                    cmd.ExecuteNonQuery();
                }

                //Products are logged for analysis
                using (var cmd = new NpgsqlCommand(@"with data as (select itemid,itemname, tradein ,quantity ,sellprice, storeid  from sales )   
                                                insert into product_log (itemid,itemname,stock,quantity,currentstock,sellprice,storeid ,createdon)
                                                select 
                                                data.itemid,
                                                data.itemname,
                                               	(case when instock is null then 0 else instock end + case when inrepair is null then 0 else inrepair end)+(case when tradein = true then -1*data.quantity else data.quantity end) stock,                                               	
                                                data.quantity,                                                 
                                               	(case when instock is null then 0 else instock end + case when inrepair is null then 0 else inrepair end) currentstock,
                                                data.sellprice,
                                                data.storeid ,
                                                current_timestamp 
                                                from data                                                         
                                                left join vw_product vp on data.storeid = vp.storeid and data.itemid = vp.id  
                                                where data.storeid = :storeId and vp.storeid = data.storeid ", connection))
                {
                    cmd.Parameters.AddWithValue("storeId", storeId);
                    cmd.ExecuteNonQuery();
                }

                //Items sold from the temporary table are deleted
                using (var cmd = new NpgsqlCommand(@"delete from sales where storeid = :storeId  ", connection))
                {
                    cmd.Parameters.AddWithValue("storeId", storeId);
                    cmd.ExecuteNonQuery();
                }

                //Recording of the personnel creating the invoice
                using (var cmd = new NpgsqlCommand(@" insert into  personnel_validate (personnelcode, validatetime,validateday,description)
                            values(:personnelCode,current_timestamp,current_date,'Create Invoice')", connection))
                {
                    cmd.Parameters.AddWithValue("personnelCode", personnelCode);
                    cmd.ExecuteNonQuery();

                };

                TwilioClient.Init("AC2c5b50cdd058de5285f255a906991ed2", "e74dbc38dbf12287d264a2e1e122df2c");

                //Sending sms to customer
                if (customerPhone != null)
                {

                    var message = MessageResource.Create(
                        body: "Welcome to the Game Platform world. Please fill in the information in the link to send your order to your e-mail. https://bit.ly/3MwNXqL",
                        from: new Twilio.Types.PhoneNumber("+19032705493"),
                        to: new Twilio.Types.PhoneNumber(customerPhone.Trim().ToString())
                    );
                }


                //Calculation of the total amounts of the relevant invoice
                using (var cmd = new NpgsqlCommand(@"select max(invoiceid) , sum(grandtotal) from invoice i 
                                                    where storeid = :storeId 
                                                    group by invoiceid 
                                                    order by invoiceid desc limit 1", connection))
                {
                    cmd.Parameters.AddWithValue("storeId", storeId);
                    cmd.Prepare();
                    NpgsqlDataReader reader = cmd.ExecuteReader();


                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            invoice =Convert.ToDouble(reader[0]);
                            grandTotal = Math.Round(Convert.ToDouble(reader[1]),2);
                        };
                    }
                    else
                    {
                    }
                    reader.Close();


                }


            }

            connection.Close();

            List<dynamic> invoiceResult = new List<dynamic>() { grandTotal, invoice }; 

            return invoiceResult;

          
            
        }

        
        [HttpPost]
        public dynamic UploadFile(IFormFile file,string personnelId, double storeId, string permission)
        {
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();
            var folderPath = "";
            var query = "";
            if (Path.GetExtension(file.FileName) == ".xlsx")
            {
                 folderPath = Path.Combine(_environment.WebRootPath, "Docs");
            }
            else if (Path.GetExtension(file.FileName) == ".txt")
            {
                folderPath = Path.Combine(_environment.WebRootPath, "Docs");
            }
            else
            {
                folderPath = Path.Combine(_environment.WebRootPath, "images", "productphotos") ;
            }
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, file.FileName);

            if (file.Length > 0 && Path.GetExtension(file.FileName) == ".xlsx"  && permission == "Admin")
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                    {

                        file.CopyTo(stream);
                            stream.Flush();

                        using (XLWorkbook wb = new XLWorkbook(stream))
                        {

                            var ws = wb.Worksheets.First();
                            var range = ws.RangeUsed();
                            for (int i = 2; i < range.RowCount() + 1; i++)
                            {
                                if (ws.Cell(i, 1).Value.ToString() == "")
                                {
                                    query = @"insert into generate_product (description,createdon) 
                                                            values(:itemName, current_timestamp)
                                                            ON CONFLICT(description) DO UPDATE set updatedon = current_timestamp";
                                    using (var cmd = new NpgsqlCommand(query, connection))
                                    {
                                        cmd.Parameters.AddWithValue("itemName", ws.Cell(i, 2).Value).ToString();
                                        cmd.Parameters.AddWithValue("storeId", Convert.ToDouble(ws.Cell(i, 7).Value));
                                        cmd.ExecuteNonQuery();
                                    }

                                    using (var cmd = new NpgsqlCommand(@$"INSERT INTO  product  (itemid,itemname,departmentid,categoryid,instock,inrepair,storeid,sellprice,image,createdon,updatedon) 
                                                          (select gp.id::text ,
                                                          gp.description ,
                                                          :departmentid,
                                                          :categoryid,
                                                          :inStock,
                                                          :inrepair,
                                                          :storeid,
                                                          :sellprice,
                                                          cast('yourUrl/ProductPhotos/'as text) || cast(gp.id as text) || cast('.jpg' as text),
                                                          current_timestamp,  
                                                          current_timestamp from generate_product gp order by gp.id desc limit 1 )", connection))
                                    {
                                        cmd.Parameters.AddWithValue("itemname", ws.Cell(i, 2).Value).ToString();
                                        cmd.Parameters.AddWithValue("departmentid", Convert.ToDouble(ws.Cell(i, 3).Value));
                                        cmd.Parameters.AddWithValue("categoryid", Convert.ToDouble(ws.Cell(i, 4).Value));
                                        cmd.Parameters.AddWithValue("inStock", Convert.ToDouble(ws.Cell(i, 5).Value));
                                        cmd.Parameters.AddWithValue("inrepair", Convert.ToDouble(ws.Cell(i, 6).Value));
                                        cmd.Parameters.AddWithValue("storeid", Convert.ToDouble(ws.Cell(i, 7).Value));
                                        cmd.Parameters.AddWithValue("sellprice", Convert.ToDouble(ws.Cell(i, 8).Value));
                                        cmd.ExecuteNonQuery();

                                    }

                                    var barcodeQuery = @"insert into  product_barcode (itemid,itemname,sellprice,storeid,itemungenerated,createdon,updatedon)
                                                values((select max(id) from generate_product limit 1), :itemName , :sellPrice, :storeId ,:inStock, current_timestamp,current_timestamp )";

                                    using (var cmd = new NpgsqlCommand(barcodeQuery, connection))
                                    {
                                        cmd.Parameters.AddWithValue("itemName", ws.Cell(i, 2).Value).ToString();
                                        cmd.Parameters.AddWithValue("inStock", Convert.ToDouble(ws.Cell(i, 5).Value));
                                        cmd.Parameters.AddWithValue("sellPrice", Convert.ToDouble(ws.Cell(i, 8).Value));
                                        cmd.Parameters.AddWithValue("storeId", Convert.ToDouble(ws.Cell(i, 7).Value));
                                        cmd.ExecuteNonQuery();
                                    }
                                }
                                else
                                {
                                    using (var cmd = new NpgsqlCommand(@$"INSERT INTO  product  (itemid,itemname,departmentid,categoryid,instock,inrepair,storeid,sellprice,image,createdon,updatedon) 
                                                          values (:itemid ,
                                                          :itemname,
                                                          :departmentid,
                                                          :categoryid,
                                                          :instock,
                                                          :inrepair,
                                                          :storeid,
                                                          :sellprice,
                                                          :image,
                                                          current_timestamp,  
                                                          current_timestamp)  
                                                        ON CONFLICT (itemid,storeid) 
                                                        DO UPDATE SET
                                                              itemname = :itemname,
                                                              departmentid = :departmentid, 
                                                             categoryid = :categoryid, 
                                                             instock = :instock, 
                                                             inrepair = :inrepair,
                                                             sellprice = :sellprice,
                                                             image = :image,
                                                             updatedon = current_timestamp  ", connection))
                                    {
                                        cmd.Parameters.AddWithValue("itemid", ws.Cell(i, 1).Value.ToString());
                                        cmd.Parameters.AddWithValue("itemname", ws.Cell(i, 2).Value.ToString());
                                        cmd.Parameters.AddWithValue("departmentid", ws.Cell(i, 3).Value == "" ? 0 : Convert.ToDouble(ws.Cell(i, 3).Value));
                                        cmd.Parameters.AddWithValue("categoryid", ws.Cell(i, 4).Value == "" ? 0 : Convert.ToDouble(ws.Cell(i, 4).Value));
                                        cmd.Parameters.AddWithValue("instock", Convert.ToDouble(ws.Cell(i, 5).Value));
                                        cmd.Parameters.AddWithValue("inrepair", ws.Cell(i, 6).Value == "" ? 0 : Convert.ToDouble(ws.Cell(i, 6).Value));
                                        cmd.Parameters.AddWithValue("storeid", Convert.ToDouble(ws.Cell(i, 7).Value));
                                        cmd.Parameters.AddWithValue("sellprice", ws.Cell(i, 8).Value == "" ? 0 : Convert.ToDouble(ws.Cell(i, 8).Value));
                                        cmd.Parameters.AddWithValue("image", $"yourUrl/ProductPhotos/{ws.Cell(i, 1).Value.ToString()}.jpg");
                                        cmd.ExecuteNonQuery();

                                    }

                                    var barcodeQuery = @"insert into  product_barcode (itemid,itemname,sellprice,storeid,itemungenerated,createdon,updatedon)
                                                values(:itemid, :itemName , :sellPrice, :storeId ,:inStock, current_timestamp,current_timestamp )
                                                on conflict(itemid,storeid)
                                                do update set
                                                itemungenerated =  product_barcode.itemungenerated + :inStock,
                                                updatedon = current_timestamp ";

                                    using (var cmd = new NpgsqlCommand(barcodeQuery, connection))
                                    {
                                        cmd.Parameters.AddWithValue("itemid", ws.Cell(i, 1).Value.ToString());
                                        cmd.Parameters.AddWithValue("itemName", ws.Cell(i, 2).Value.ToString());
                                        cmd.Parameters.AddWithValue("inStock", Convert.ToDouble(ws.Cell(i, 5).Value));
                                        cmd.Parameters.AddWithValue("sellPrice", ws.Cell(i, 8).Value == "" ? 0 : Convert.ToDouble(ws.Cell(i, 8).Value));
                                        cmd.Parameters.AddWithValue("storeId", Convert.ToDouble(ws.Cell(i, 7).Value));
                                        cmd.ExecuteNonQuery();
                                    }

                                }

                            }

                            


                        }
                
                }                
            }
            else if (file.Length > 0 && Path.GetExtension(file.FileName) == ".txt")
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                        stream.Flush();
                }
                FileStream fileStream = new FileStream(filePath, FileMode.Open);
                StreamReader streamReader = new StreamReader(fileStream);
                string reader = streamReader.ReadToEnd();
                List<string> lines = new List<string>(reader.Split("\r\n"));
                lines.ForEach(line =>
                {
                    using (var cmd = new NpgsqlCommand(@"insert into  weekly_report_log (itemid,personnelcode,itemcount,storeid,createdon)
                                                        values(:itemId,:personnelCode,1,:storeId ,current_timestamp)
                                                        on conflict (itemid,storeid)
                                                        do update
                                                        set
                                                        itemcount =  weekly_report_log.itemcount + 1,
                                                        updatedon  = current_timestamp ", connection))
                    {
                        cmd.Parameters.AddWithValue("itemId", line);
                        cmd.Parameters.AddWithValue("personnelCode", personnelId);
                        cmd.Parameters.AddWithValue("storeId", storeId);
                        cmd.ExecuteNonQuery();
                    };


                    using (var cmd = new NpgsqlCommand(@"insert into  weekly_report (itemid,isselected,itemorder,storeid,createdon)
                                                    values(:itemId,true,0,:storeId,current_timestamp)
                                                    ON CONFLICT (itemid,storeid) 
                                                    DO UPDATE SET
                                                    isselected = true ,
                                                    updatedon = current_timestamp", connection))
                    {
                        cmd.Parameters.AddWithValue("itemId", line);
                        cmd.Parameters.AddWithValue("storeId", storeId);
                        cmd.ExecuteNonQuery();
                    };
                });
                fileStream.Dispose();
                System.IO.File.Move(filePath, filePath.Split(".txt")[0].ToString() + DateTime.Now.Ticks + ".txt");
            }
            else
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                    stream.Flush();   
                }
            }


            connection.Close();
            
            return filePath;
        }

        
        [HttpPost]
        public dynamic Validation(string cashierId)
        {

            List<ValidationViewModel> list = new List<ValidationViewModel>();
            //log table
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();
            var response = "";
            using (var cmd = new NpgsqlCommand(@" select ps.id ,ps.description , pp.userpermission  
                                                    from  personnel pp
                                                    left join  store ps on ps.id = pp.storeid 
                                                    where personnelcode  = :personnelCode ", connection))
            {
                cmd.Parameters.AddWithValue("personnelCode", cashierId);

                NpgsqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        list.Add(new ValidationViewModel
                        {
                            id = Convert.ToDouble(reader[0] == DBNull.Value ? 0 : reader[0]),
                            description = reader[1]?.ToString(),
                            permission = reader[2]?.ToString(),
                        });
                    }
                }
                else
                {
                }
                reader.Close();

            };

            if (response != null)
            {
                using (var cmd = new NpgsqlCommand(@" insert into  personnel_validate (personnelcode, validatetime,validateday)
                            values(:personnelCode,current_timestamp,current_date)", connection))
                {
                    cmd.Parameters.AddWithValue("personnelCode", cashierId);
                    cmd.ExecuteNonQuery();

                };
            }
            
            connection.Close();

           
            return list;
        }


        [HttpPut]
        [Consumes("application/x-www-form-urlencoded")]
        public dynamic UpdateWeeklyReport([FromForm] string values, string key, string cashierId, double storeId)
        {

            //log table
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();
            var weeklyReport = JsonConvert.DeserializeObject<UpdateWeeklyInventoryViewModel>(values);

            using (var cmd = new NpgsqlCommand(@"insert into  weekly_report_log (itemid,personnelcode,itemcount,storeid,createdon)
                                                        values(:itemId,:personnelCode,:itemCount,:storeId ,current_timestamp)
                                                        on conflict (itemid,storeid)
                                                        do update
                                                        set
                                                        itemcount = :itemCount,
                                                        updatedon  = current_timestamp ", connection))
            {
                cmd.Parameters.AddWithValue("itemCount", weeklyReport.count == null ? 0 : weeklyReport.count);
                cmd.Parameters.AddWithValue("itemId", key);
                cmd.Parameters.AddWithValue("personnelCode", cashierId);
                cmd.Parameters.AddWithValue("storeId", storeId);
                cmd.ExecuteNonQuery();

            }
            connection.Close();
            return true;
        }


        [HttpPut]
        [Consumes("application/x-www-form-urlencoded")]
        public dynamic UpdateSaleInfo([FromForm] string values, string key)
        {

            //log table
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();
            var saleInfo = JsonConvert.DeserializeObject<SaleInfoViewModel>(values);
            var saleInfoKey = JsonConvert.DeserializeObject<SaleInfoViewModel>(key);

            using (var cmd = new NpgsqlCommand(@"update sales 
                                                set
                                                tradein = case when :tradeIn is null then tradein else :tradeIn end,
                                                isdiscount = :isDiscount ,
                                                torepair = case when :toRepair is null then torepair else :toRepair end ,
                                                oldsellprice = 
                                                case 
	                                                when oldsellprice is null then sellprice 
                                                else oldsellprice  end,
                                                sellprice  = 
                                                case
	                                                when :sellPrice > 0 then :sellPrice
	                                                when :tradeIn = true then sellprice  * -1
	                                                when :tradeIn = true or :toRepair = true then sellprice  * -1 
	                                                when :tradeIn = false and oldsellprice is not null then oldsellprice 
	                                                when itemid = (select distinct  vp.id from vw_product vp where vp.id  = :itemId) and :isDiscount = true and :tradeIn = false
	                                                then sellprice * (select distinct case when discountrate is null then 85 else discountrate end  
	                                                from vw_product  pd 
	                                                left join  product_discount vp on pd.id =vp.itemid  
	                                                where pd.id  = :itemId)/100
	                                                else sellprice end
                                                where id = :id   ", connection))
            {
                cmd.Parameters.AddWithValue("tradeIn", saleInfo.tradeIn == null ? DBNull.Value : saleInfo.tradeIn);
                cmd.Parameters.AddWithValue("id", saleInfoKey.id);
                cmd.Parameters.AddWithValue("itemId", saleInfoKey.itemId);
                cmd.Parameters.AddWithValue("isDiscount", saleInfo.isDiscount == null ? DBNull.Value : saleInfo.isDiscount);
                cmd.Parameters.AddWithValue("toRepair", saleInfo.toRepair == null ? DBNull.Value : saleInfo.toRepair);
                cmd.Parameters.AddWithValue("sellPrice", saleInfo.grandTotal == null ? 0 : saleInfo.grandTotal);
                cmd.ExecuteNonQuery();

            }
            connection.Close();
            return true;
        }

        [HttpPost]
        public dynamic Permission(string cashierId)
        {

            List<ValidationViewModel> list = new List<ValidationViewModel>();
            //log table
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();
            var response = "";
            using (var cmd = new NpgsqlCommand(@"  select userpermission 
                                                    from  personnel pp
                                                    where personnelcode  = :personnelCode and userpermission  != 'Employee'", connection))
            {
                cmd.Parameters.AddWithValue("personnelCode", cashierId);

                response = cmd.ExecuteScalar()?.ToString();

               

            };           

            connection.Close();


            return response == null ? false : true;
        }

        [HttpPost]
        public void PrintProduct(double itemId, string itemName, double sellPrice, double storeId)
        {
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();


            using (var command = new NpgsqlCommand(@"INSERT into  product_barcode  (itemid,itemname,itemungenerated,sellprice ,storeid, createdon)
                                                        values(:itemId,
                                                        :itemName,
                                                        1,
                                                        :sellPrice,
                                                        :storeId,
                                                        current_timestamp)
                                                        ON CONFLICT(itemid, storeid)
                                                        DO UPDATE 
                                                        SET
                                                        itemungenerated  =  product_barcode.itemungenerated + 1,
                                                        updatedon = current_timestamp", connection))
            {
                command.Parameters.AddWithValue("itemId", itemId);
                command.Parameters.AddWithValue("itemName", itemName);
                command.Parameters.AddWithValue("sellPrice", sellPrice);
                command.Parameters.AddWithValue("storeId", storeId);
                command.ExecuteNonQuery();
            }
            connection.Close();
            return;
        }


        [HttpDelete]
        [Consumes("application/x-www-form-urlencoded")]
        public dynamic DeleteSaleInfo([FromForm] string key)
        {

            //log table
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            var saleInfoKey = JsonConvert.DeserializeObject<SaleInfoViewModel>(key);


            using (var cmd = new NpgsqlCommand(@"delete from sales where id = :id", connection))
            {
                cmd.Parameters.AddWithValue("id",Convert.ToDouble(saleInfoKey?.id));
                cmd.ExecuteNonQuery();

            }
            connection.Close();
            return true;
        }

        public byte[] ConvertImageToByte(Image img)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                img.Save(memoryStream, ImageFormat.Png);
                return memoryStream.ToArray();

            }
        }


    }

}