using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using Npgsql;
using System.Diagnostics;
using System.Runtime.Serialization;
using static Test_InventoryPage.ViewModels;

namespace Test_InventoryPage.Pages
{
    //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    //[IgnoreAntiforgeryToken]
    [ApiController]
    [Route("[controller]/[action]")]
    public class Admin : ControllerBase
    {

        [IgnoreDataMember] 
        public string? RequestId { get; set; }

        [IgnoreDataMember]
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        private readonly ILogger<Admin> _logger;

        public Admin(ILogger<Admin> logger)
        {
            _logger = logger;
        }


        [HttpGet]
        public dynamic PersonnelInformation(string? key)
        {
            List<PersonnelInformationViewModel> personnelInformationViewModel = new List<PersonnelInformationViewModel>();
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@"select personnelcode , storeid  from  personnel pp where personnelcode = :personnelCode", connection))
            {
                cmd.Parameters.AddWithValue("personnelCode", key == null ? "" : key);
                cmd.Prepare();
                NpgsqlDataReader reader = cmd.ExecuteReader();


                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        personnelInformationViewModel.Add(new PersonnelInformationViewModel
                        {
                            personnelCode = reader[0]?.ToString(),
                            storeId = Convert.ToDouble(reader[1] == DBNull.Value ? 0 : reader[1]),
                        });
                    }
                }
                else
                {
                }
                reader.Close();


            }
            connection.Close();

            return personnelInformationViewModel;

        }


        [HttpGet]
        public dynamic ReportStoreList(double? storeId)
        {
            List<ReportStoreViewModel> list = new List<ReportStoreViewModel>();
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@"select 
                                                storeid 							, 
                                                sum(sellprice) 		as sellprice 	, 
                                                sum(subtotal) 		as subtotal 	, 
                                                sum(taxtotal) 		as taxtotal 	, 
                                                sum(grandtotal) 	as grandtotal 	,
                                                date(createdon) 	as today        ,
                                                ps.description 						
                                                from invoice i
                                                join  store ps on ps.id = i.storeid 
                                    			where case when :storeId = 0  then ps.id is not null else ps.id = :storeId::numeric end  
                                                group by storeid,ps.description , date(createdon)", connection))
            {
                cmd.Parameters.AddWithValue("storeId", storeId == null ? 0 : storeId);
                cmd.Prepare();
                NpgsqlDataReader reader = cmd.ExecuteReader();


                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        list.Add(new ReportStoreViewModel
                        {
                            storeId = Convert.ToDouble(reader[0] == DBNull.Value ? 0 : reader[0]),
                            sellPrice = Convert.ToDouble(reader[1] == DBNull.Value ? 0 : reader[1]),
                            subTotal = Convert.ToDouble(reader[2] == DBNull.Value ? 0 : reader[2]),
                            taxTotal = Convert.ToDouble(reader[3] == DBNull.Value ? 0 : reader[3]),
                            grandTotal = Convert.ToDouble(reader[4] == DBNull.Value ? 0 : reader[4]),
                            getDate = Convert.ToDateTime(reader[5] == DBNull.Value ? 0 : reader[5]),
                            storeName = reader[6]?.ToString(),
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
        public dynamic ReportPersonnelList(double? storeId)
        {
            List<ReportPersonnelViewModel> list = new List<ReportPersonnelViewModel>();
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@"select 
                                                    ps.personnelcode  				    , 
                                                    sum(sellprice) 		as sellprice 	, 
                                                    sum(subtotal) 		as subtotal 	, 
                                                    sum(taxtotal) 		as taxtotal 	, 
                                                    sum(grandtotal) 	as grandtotal 	,
                                                    date(i.createdon) 	as today        ,
                                                    to_char(min(validatetime),'HH24:MI')   as clockin		,
                                                    to_char(max(validatetime),'HH24:MI')   as clockout		,
                                                    to_char((max(validatetime)-min(validatetime)),'HH24:MI') as workinghour,
                                                    ps.personnelname  					
                                                    from invoice i
                                                    join  personnel  ps on ps.personnelcode = i.personnelid::text 
                                                    join  personnel_validate ppv on ppv.personnelcode = i.personnelid::text and date(i.createdon) = date(ppv.validateday)                                                   
                                    			  	where case when :storeId = 0  then ps.storeid  is not null else ps.storeid  = :storeId::numeric end  
                                                    group by ps.personnelcode ,ps.personnelname  , date(i.createdon)", connection))
            {
                cmd.Parameters.AddWithValue("storeId", storeId == null ? 0 : storeId);
                cmd.Prepare();
                NpgsqlDataReader reader = cmd.ExecuteReader();


                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        list.Add(new ReportPersonnelViewModel
                        {
                            personnelCode = reader[0]?.ToString(),
                            sellPrice = Convert.ToDouble(reader[1] == DBNull.Value ? 0 : reader[1]),
                            subTotal = Convert.ToDouble(reader[2] == DBNull.Value ? 0 : reader[2]),
                            taxTotal = Convert.ToDouble(reader[3] == DBNull.Value ? 0 : reader[3]),
                            grandTotal = Convert.ToDouble(reader[4] == DBNull.Value ? 0 : reader[4]),
                            getDate = Convert.ToDateTime(reader[5] == DBNull.Value ? 0 : reader[5]),
                            clockIn = reader[6]?.ToString(),
                            clockOut = reader[7]?.ToString(),
                            workingHour = reader[8]?.ToString(),
                            personnelName = reader[9]?.ToString(),
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
        public dynamic ReportInvoiceList(double? storeId)
        {
            List<ReportInvoiceViewModel> list = new List<ReportInvoiceViewModel>();
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@"select 
                                                i.invoiceid  						, 
                                                sum(sellprice) 		as sellprice 	, 
                                                sum(subtotal) 		as subtotal 	, 
                                                sum(taxtotal) 		as taxtotal 	, 
                                                sum(grandtotal) 	as grandtotal 	,
                                                date(i.createdon) 	as today  		,
                                                to_char(i.createdon,'HH24:MM'),
                                                customerphone 
                                                from invoice i
                                                where case when :storeId = 0  then i.storeid  is not null else i.storeid  = :storeId::numeric end 
                                                group by invoiceid , date(i.createdon), to_char(i.createdon,'HH24:MM'), customerphone ", connection))
            {
                cmd.Parameters.AddWithValue("storeId", storeId == null ? 0 : storeId);
                cmd.Prepare();
                NpgsqlDataReader reader = cmd.ExecuteReader();


                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        list.Add(new ReportInvoiceViewModel
                        {
                            invoiceId = Convert.ToDouble(reader[0] == DBNull.Value ? 0 : reader[0]),
                            sellPrice = Convert.ToDouble(reader[1] == DBNull.Value ? 0 : reader[1]),
                            subTotal = Convert.ToDouble(reader[2] == DBNull.Value ? 0 : reader[2]),
                            taxTotal = Convert.ToDouble(reader[3] == DBNull.Value ? 0 : reader[3]),
                            grandTotal = Convert.ToDouble(reader[4] == DBNull.Value ? 0 : reader[4]),
                            getDate = Convert.ToDateTime(reader[5] == DBNull.Value ? 0 : reader[5]),
                            customerPhone = reader[6].ToString() == DBNull.Value.ToString() ? "" : reader[6].ToString(),
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
        public dynamic ReportInvoiceDetailList(double? invoiceId = null)
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
                                                    where invoiceid = :invoiceId", connection))
            {
                cmd.Parameters.AddWithValue("invoiceId", invoiceId == null ? 0 : invoiceId);
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
                            quantity = Convert.ToDouble(reader[3] == DBNull.Value ? null : reader[3] ),
                            personnelName = reader[4].ToString() == DBNull.Value.ToString() ? null : reader[4].ToString(),
                            storeName = reader[5].ToString() == DBNull.Value.ToString() ? null : reader[5].ToString(),
                            paymentMethod = reader[6].ToString() == DBNull.Value.ToString() ? null : reader[6].ToString(),
                            sellPrice = Convert.ToDouble(reader[7] == DBNull.Value ? null : reader[7] ),
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

        [HttpGet]
        public dynamic ReportCustomerList()
        {
            List<ReportCustomerViewModel> list = new List<ReportCustomerViewModel>();
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@"select 
                                                id,
                                                firstname ,
                                                lastname ,
                                                email ,
                                                phonenumber ,
                                                date(createdon)
                                                from  customer pc ", connection))
            {
                cmd.Prepare();
                NpgsqlDataReader reader = cmd.ExecuteReader();


                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        list.Add(new ReportCustomerViewModel
                        {
                            id = Convert.ToDouble(reader[0] == DBNull.Value ? 0 : reader[0]),
                            firstName = reader[1]?.ToString(),
                            lastName = reader[2]?.ToString(),
                            email = reader[3]?.ToString(),
                            phoneNumber = reader[4]?.ToString(),
                            getDate = Convert.ToDateTime(reader[5] == DBNull.Value ? 0 : reader[5]),
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
        public dynamic ReportStockList(double? storeId)
        {
            List<ReportStockViewModel> list = new List<ReportStockViewModel>();
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@"select 
                                                itemid,
                                                itemname,
                                                stock,
                                                quantity,
                                                currentstock,
                                                sellprice,
                                                ps.description ,
                                                date(createdon),
                                                TO_CHAR(createdon,'HH24:MI:SS')
                                                from product_log pl 
                                                join  store ps on ps.id = pl.storeid
                                                where case when :storeId = 0  then ps.id  is not null else ps.id  = :storeId::numeric end  ", connection))
            {
                cmd.Parameters.AddWithValue("storeId", storeId == null ? 0 : storeId);
                cmd.Prepare();
                NpgsqlDataReader reader = cmd.ExecuteReader();


                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        list.Add(new ReportStockViewModel
                        {
                            itemId = Convert.ToDouble(reader[0] == DBNull.Value ? 0 : reader[0]),
                            itemName = reader[1]?.ToString(),
                            stock = Convert.ToDouble(reader[2] == DBNull.Value ? 0 : reader[2]),
                            quantity = Convert.ToDouble(reader[3] == DBNull.Value ? 0 : reader[3]),
                            currentStock = Convert.ToDouble(reader[4] == DBNull.Value ? 0 : reader[4]),
                            sellPrice = Convert.ToDouble(reader[5] == DBNull.Value ? 0 : reader[5]),
                            store = reader[6]?.ToString(),
                            getDate = Convert.ToDateTime(reader[7] == DBNull.Value ? 0 : reader[7]),
                            getDateHour = reader[8]?.ToString(),
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
        public dynamic ReportProductList(double? storeId)
        {
            List<ReportProductViewModel> list = new List<ReportProductViewModel>();
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@"select productid, productname , sum(quantity) as quantity,   (date(max(createdon))-date(min(createdon)))::numeric  as daterange 
                                                        from invoice i 
                                                        where case when :storeId = 0  then i.storeid  is not null else i.storeid  = :storeId::numeric end 
                                                        group by productid,productname ", connection))
            {
                cmd.Parameters.AddWithValue("storeId", storeId == null ? 0 : storeId);
                cmd.Prepare();
                NpgsqlDataReader reader = cmd.ExecuteReader();


                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        list.Add(new ReportProductViewModel
                        {
                            itemId = Convert.ToDouble(reader[0] == DBNull.Value ? 0 : reader[0]),
                            itemName = reader[1]?.ToString(),
                            quantity = Convert.ToDouble(reader[2] == DBNull.Value ? 0 : reader[2]),
                            dateRange = Convert.ToDouble(reader[3] == DBNull.Value ? 0 : reader[3]),
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
        public dynamic ReportWeeklyOrderList(double? storeId)
        {
            List<ReportWeeklyOrderViewModel> list = new List<ReportWeeklyOrderViewModel>();
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@"select distinct
                                                    pwr.itemid ,
                                                    vp.title ,
                                                    wrl.itemcount ,
                                                    vp.instock ,
                                                    ps.description ,
                                                    pwr.createdon ,
                                                    pwr.itemorder 
                                                    from  weekly_report  pwr
                                                    join vw_product vp on vp.id = pwr.itemid
                                                    join  store ps on ps.id = pwr.storeid
                                                    join  weekly_report_log wrl on wrl.itemid = pwr.itemid 
                                                    where  isselected = true 
                                                    and vp.instock > 0 
                                                    and case when :storeId = 0  then pwr.storeid is not null else pwr.storeid   = :storeId::numeric end 
                                                    and date_trunc('week', current_date) - interval '1 day' < pwr.createdon  ", connection))
            {
                cmd.Parameters.AddWithValue("storeId", storeId == null ? 0 : storeId);
                cmd.Prepare();
                NpgsqlDataReader reader = cmd.ExecuteReader();


                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        list.Add(new ReportWeeklyOrderViewModel
                        {
                            itemId = Convert.ToDouble(reader[0] == DBNull.Value ? 0 : reader[0]),
                            itemName = reader[1]?.ToString(),
                            itemCount = Convert.ToDouble(reader[2] == DBNull.Value ? 0 : reader[2]),
                            inStock = Convert.ToDouble(reader[3] == DBNull.Value ? 0 : reader[3]),
                            store = reader[4]?.ToString(),
                            getDate = Convert.ToDateTime(reader[5] == DBNull.Value ? 0 : reader[5]),
                            itemOrder = Convert.ToDouble(reader[6] == DBNull.Value ? 0 : reader[6]),
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


        [HttpPost]
        public dynamic UpdateProductReport()
        {

            //log table
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@"insert into  undercount (itemid,undercount,createdon,storeid)                                                
select distinct 
                                                    pwr.itemid ,
                                                    pp2.instock  - pwrl.itemcount  ,
                                                    current_timestamp ,
                                                    pwr.storeid 
                                                    from  weekly_report pwr
                                                    left join  weekly_report_log pwrl on pwrl.itemid = pwr.itemid
                                                    left join  product pp2 on pp2.itemid = pwr.itemid
                                                    where pp2.instock > 0 and pwrl.itemcount > 0 ", connection))
            {
                cmd.ExecuteNonQuery();
            }

            using (var cmd = new NpgsqlCommand(@"with data as (
                                select pwr.itemid , max(case when pwr.updatedon is null then pwr.createdon  else pwr.updatedon end) , max(wrl.itemcount ) as itemcount
                                                    from  weekly_report pwr
                                                    join  weekly_report_log wrl  on wrl.itemid = pwr.itemid 
                                                    group by pwr.itemid )
                                                    update  product pp 
                                                    set
                                                    instock = data.itemcount
                                                    from 
                                                    data
                                                    where 
                                                    pp.itemid = data.itemid", connection))
            {
                cmd.ExecuteNonQuery();
            }

            using (var cmd = new NpgsqlCommand(@"update  weekly_report set itemorder = 1", connection))
            {
                cmd.ExecuteNonQuery();
            }

            using (var cmd = new NpgsqlCommand(@"update  weekly_report_log pwrl set itemcount = 0 ", connection))
            {
                cmd.ExecuteNonQuery();
            }

            connection.Close();
            return true;
        }

        [HttpGet]
        public dynamic ReportUndercountList(double? storeId)
        {
            List<ReportUndercountViewModel> list = new List<ReportUndercountViewModel>();
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@"select  distinct 
                                                    pu.itemid,
                                                    vp.title ,
                                                    pu.undercount,
                                                    ps.description ,
                                                    pu.createdon 
                                                    from  undercount pu
                                                    join vw_product vp on vp.id = pu.itemid
                                                    join  store ps on ps.id = pu.storeid
                                                    where case when :storeId = 0  then ps.id  is not null else ps.id  = :storeId::numeric end 
                                                    and pu.undercount > 0 and vp.instock > 0 ", connection))
            {
                cmd.Parameters.AddWithValue("storeId", storeId == null ? 0 : storeId);
                cmd.Prepare();
                NpgsqlDataReader reader = cmd.ExecuteReader();


                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        list.Add(new ReportUndercountViewModel
                        {
                            itemId = reader[0]?.ToString(),
                            itemName = reader[1]?.ToString(),
                            undercount = Convert.ToDouble(reader[2] == DBNull.Value ? 0 : reader[2]),
                            store = reader[3]?.ToString(),
                            getDate = Convert.ToDateTime(reader[4] == DBNull.Value ? 0 : reader[4]),
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
        public dynamic ReportProductRequestList(double? storeId)
        {
            List<ReportProductRequestViewModel> reportProductRequest = new List<ReportProductRequestViewModel>();
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@"select 
                                                 vp.id,
                                                 title ,
                                                 ps.description  ,
                                                 pp.personnelname  ,
                                                 pcr.createdon 
                                                 from vw_product vp 
                                                 join  store ps on ps.id = vp.storeid 
                                                 left join  product_request pcr on pcr.itemid = vp.id 
                                                 left join  personnel pp on pp.personnelcode = pcr.personnelcode
                                                 where instock <0 and 
                                                 case when :storeId = 0  then vp.storeid  is not null else vp.storeid  = :storeId::numeric end
                                                 order by pcr.createdon desc", connection))
            {
                cmd.Parameters.AddWithValue("storeId", storeId == null ? 0 : storeId);
                cmd.Prepare();
                NpgsqlDataReader reader = cmd.ExecuteReader();


                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        reportProductRequest.Add(new ReportProductRequestViewModel
                        {
                            itemId = Convert.ToDouble(reader[0] == DBNull.Value ? 0 : reader[0]),
                            itemName = reader[1]?.ToString(),
                            store = reader[2]?.ToString(),
                            personnelName = reader[3]?.ToString(),
                            getDate = Convert.ToDateTime(reader[4] == DBNull.Value ? null : reader[4]),
                        });
                    }
                }
                else
                {
                }
                reader.Close();


            }
            connection.Close();

            return reportProductRequest;

        }

        [HttpGet]
        public dynamic ReportAccessibilityList()
        {
            List<ReportAccessibilityViewModel> reportAccessibility = new List<ReportAccessibilityViewModel>();
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@"select 
                                                ra.personnelcode,
                                                ps.description ,
                                                pp.userpermission ,
                                                ra.store,
                                                cashier ,
                                                invoice ,
                                                customer ,
                                                stock ,
                                                salesanalysis ,
                                                inventoryorder ,
                                                undercount ,
                                                productrequest 
                                                from report_accessibility ra 
                                                join  personnel pp on pp.personnelcode = ra.personnelcode
                                                left join  store ps on ps.id = pp.storeid ", connection))
            {
                cmd.Prepare();
                NpgsqlDataReader reader = cmd.ExecuteReader();


                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        reportAccessibility.Add(new ReportAccessibilityViewModel
                        {
                            personnelCode = reader[0]?.ToString(),
                            storeName = reader[1]?.ToString() == DBNull.Value.ToString() ? "GM" : reader[1]?.ToString(),
                            permission = reader[2]?.ToString(),
                            store = Convert.ToBoolean(reader[3] == DBNull.Value ? 0 : reader[3]),
                            cashier = Convert.ToBoolean(reader[4] == DBNull.Value ? 0 : reader[4]),
                            invoice = Convert.ToBoolean(reader[5] == DBNull.Value ? 0 : reader[5]),
                            customer = Convert.ToBoolean(reader[6] == DBNull.Value ? 0 : reader[6]),
                            stock = Convert.ToBoolean(reader[7] == DBNull.Value ? 0 : reader[7]),
                            salesAnalysis = Convert.ToBoolean(reader[8] == DBNull.Value ? 0 : reader[8]),
                            inventoryOrder = Convert.ToBoolean(reader[9] == DBNull.Value ? 0 : reader[9]),
                            undercount = Convert.ToBoolean(reader[10] == DBNull.Value ? 0 : reader[10]),
                            productRequest = Convert.ToBoolean(reader[11] == DBNull.Value ? 0 : reader[11]),
                        });
                    }
                }
                else
                {
                }
                reader.Close();


            }
            connection.Close();

            return reportAccessibility;

        }


        [HttpGet]
        public dynamic ReportLookupPersonnel()
        {
            List<PersonnelLookupViewModel> personnelLookupViewModel = new List<PersonnelLookupViewModel>();
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@"select personnelcode ,personnelname  from  personnel pp ", connection))
            {
                cmd.Prepare();
                NpgsqlDataReader reader = cmd.ExecuteReader();


                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        personnelLookupViewModel.Add(new PersonnelLookupViewModel
                        {
                            personnelCode = reader[0]?.ToString(),
                            personnelName = reader[1]?.ToString(),
                        });
                    }
                }
                else
                {
                }
                reader.Close();


            }
            connection.Close();

            return personnelLookupViewModel;

        }


        [HttpGet]
        public dynamic ReportPermission(string personnelCode)
        {
            List<ReportPermissionViewModel> reportPermission = new List<ReportPermissionViewModel>();
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@"select 
                                                personnelcode,
                                                store,
                                                cashier ,
                                                invoice ,
                                                customer ,
                                                stock ,
                                                salesanalysis ,
                                                inventoryorder ,
                                                undercount ,
                                                productrequest 
                                                from report_accessibility ra 
                                                where personnelcode = :personnelCode ", connection))
            {
                cmd.Parameters.AddWithValue("personnelCode", personnelCode);
                cmd.Prepare();
                NpgsqlDataReader reader = cmd.ExecuteReader();


                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        reportPermission.Add(new ReportPermissionViewModel
                        {
                            personnelCode = reader[0]?.ToString(),
                            store = Convert.ToBoolean(reader[1] == DBNull.Value ? 0 : reader[1]),
                            cashier = Convert.ToBoolean(reader[2] == DBNull.Value ? 0 : reader[2]),
                            invoice = Convert.ToBoolean(reader[3] == DBNull.Value ? 0 : reader[3]),
                            customer = Convert.ToBoolean(reader[4] == DBNull.Value ? 0 : reader[4]),
                            stock = Convert.ToBoolean(reader[5] == DBNull.Value ? 0 : reader[5]),
                            salesAnalysis = Convert.ToBoolean(reader[6] == DBNull.Value ? 0 : reader[6]),
                            inventoryOrder = Convert.ToBoolean(reader[7] == DBNull.Value ? 0 : reader[7]),
                            undercount = Convert.ToBoolean(reader[8] == DBNull.Value ? 0 : reader[8]),
                            productRequest = Convert.ToBoolean(reader[9] == DBNull.Value ? 0 : reader[9]),
                        });
                    }
                }
                else
                {
                }
                reader.Close();


            }
            connection.Close();

            return reportPermission;

        }


        [HttpGet]
        public dynamic SettingPriceList()
        {
            List<SettingPriceViewModel> list = new List<SettingPriceViewModel>();
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@"select 
                                                id,
                                                category,
                                                sellprice,
                                                ebayprice,
                                                amazonprice,
                                                createdon,
                                                updatedon                       
                                                from  updateprice pl", connection))
            {
                cmd.Prepare();
                NpgsqlDataReader reader = cmd.ExecuteReader();


                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        list.Add(new SettingPriceViewModel
                        {
                            id = Convert.ToDouble(reader[0] == DBNull.Value ? 0 : reader[0]),
                            category = reader[1]?.ToString(),
                            sellPrice = Convert.ToDouble(reader[2] == DBNull.Value ? 0 : reader[2]),
                            ebayPrice = Convert.ToDouble(reader[3] == DBNull.Value ? 0 : reader[3]),
                            amazonPrice = Convert.ToDouble(reader[4] == DBNull.Value ? 0 : reader[4]),
                            createDate = Convert.ToDateTime(reader[5] == DBNull.Value ? 0 : reader[5]),
                            updateDate = Convert.ToDateTime(reader[6] == DBNull.Value ? 0 : reader[6]),
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
        public dynamic SettingWeeklyInventoryList()
        {
            List<SettingWeeklyInventoryViewModel> list = new List<SettingWeeklyInventoryViewModel>();
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@"select 
                                            vp.id  , 
                                            vp.title , 
                                            wr.isselected , 
                                            vp.productgroup , 
                                            wr.itemorder , 
                                            ps.description , 
                                            max(vp.category)  
                                            from vw_product vp 
                                            left join weekly_report  wr on vp.id = wr.itemid 
                                            left join  store ps on ps.id  = wr.storeid 
                                            group by vp.id , vp.title , wr.isselected ,vp.productgroup , wr.itemorder , ps.description 
                                            order by wr.isselected asc   ", connection))
            {
                cmd.Prepare();
                NpgsqlDataReader reader = cmd.ExecuteReader();


                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        list.Add(new SettingWeeklyInventoryViewModel
                        {
                            id = reader[0]?.ToString(),
                            itemName = reader[1]?.ToString(),
                            isSelected = Convert.ToBoolean(reader[2] == DBNull.Value ? false : reader[2]),
                            productGroup = reader[3]?.ToString(),
                            itemOrder = Convert.ToDouble(reader[4] == DBNull.Value ? false : reader[4]),
                            storeName = reader[5]?.ToString(),
                            category = reader[6]?.ToString(),
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
        public dynamic SettingPromotionList()
        {
            List<SettingPromotionViewModel> list = new List<SettingPromotionViewModel>();
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@"select 
                                                    id,
                                                    promotionname,
                                                    startdate,
                                                    enddate
                                                    from  promotion ", connection))
            {
                cmd.Prepare();
                NpgsqlDataReader reader = cmd.ExecuteReader();


                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        list.Add(new SettingPromotionViewModel
                        {
                            id = Convert.ToDouble(reader[0] == DBNull.Value ? 0 : reader[0]),
                            promotionName = reader[1]?.ToString(),
                            startDate = Convert.ToDateTime(reader[2] == DBNull.Value ? false : reader[2]),
                            endDate = Convert.ToDateTime(reader[3] == DBNull.Value ? false : reader[3]),
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
        public dynamic SettingDiscountList()
        {
            List<SettingDiscountViewModel> settingDiscountViewModels = new List<SettingDiscountViewModel>();
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@"select 
                                                    distinct
                                                    vp.id ,
                                                    vp.title ,
                                                    case when ppd.discountrate is null then 85 else ppd.discountrate end
                                                    from vw_product vp 
                                                    left join  product_discount ppd on ppd.itemid = vp.id  ", connection))
            {
                cmd.Prepare();
                NpgsqlDataReader reader = cmd.ExecuteReader();


                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        settingDiscountViewModels.Add(new SettingDiscountViewModel
                        {
                            id = reader[0]?.ToString(),
                            itemName = reader[1]?.ToString(),
                            discountRate = Convert.ToDouble(reader[2] == DBNull.Value ? 0 : reader[2]),
                        });
                    }
                }
                else
                {
                }
                reader.Close();


            }
            connection.Close();

            return settingDiscountViewModels;

        }


        [HttpGet]
        public dynamic SettingLookupCategory()
        {
            List<SettingUpdatePrice> list = new List<SettingUpdatePrice>();
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@"select id, description  from  category pc", connection))
            {
                cmd.Prepare();
                NpgsqlDataReader reader = cmd.ExecuteReader();


                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        list.Add(new SettingUpdatePrice
                        {
                            id = Convert.ToDouble(reader[0]),
                            description = reader[1]?.ToString(),
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
        public dynamic SettingAccessibilityList()
        {
            List<SettingAccessibilityViewModel> settingAccessibilityViewModel = new List<SettingAccessibilityViewModel>();
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@"select 
                                                sa.personnelcode,
                                                ps.description ,
                                                pp.userpermission ,
                                                price ,
                                                inventoryorder  ,
                                                promotions  
                                                from setting_accessibility sa  
                                                join  personnel pp on pp.personnelcode = sa.personnelcode
                                                left join  store ps on ps.id = pp.storeid", connection))
            {
                cmd.Prepare();
                NpgsqlDataReader reader = cmd.ExecuteReader();


                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        settingAccessibilityViewModel.Add(new SettingAccessibilityViewModel
                        {
                            personnelCode = reader[0]?.ToString(),
                            storeName = reader[1]?.ToString() == "" ? "GM" : reader[1]?.ToString(),
                            permission = reader[2]?.ToString(),
                            price = Convert.ToBoolean(reader[3] == DBNull.Value ? 0 : reader[3]),
                            inventoryOrder = Convert.ToBoolean(reader[4] == DBNull.Value ? 0 : reader[4]),
                            promotions = Convert.ToBoolean(reader[5] == DBNull.Value ? 0 : reader[5]),
                        });
                    }
                }
                else
                {
                }
                reader.Close();


            }
            connection.Close();

            return settingAccessibilityViewModel;

        }


        [HttpGet]
        public dynamic SettingPermission(string personnelCode)
        {
            List<SettingPermissionViewModel> settingPermission = new List<SettingPermissionViewModel>();
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@"select 
                                                    personnelcode ,
                                                    price ,
                                                    inventoryorder ,
                                                    promotions ,
                                                    discount
                                                    from setting_accessibility sa 
                                                    where personnelcode = :personnelCode ", connection))
            {
                cmd.Parameters.AddWithValue("personnelCode", personnelCode);
                cmd.Prepare();
                NpgsqlDataReader reader = cmd.ExecuteReader();


                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        settingPermission.Add(new SettingPermissionViewModel
                        {
                            personnelCode = reader[0]?.ToString(),
                            price = Convert.ToBoolean(reader[1] == DBNull.Value ? 0 : reader[1]),
                            inventoryOrder = Convert.ToBoolean(reader[2] == DBNull.Value ? 0 : reader[2]),
                            promotions = Convert.ToBoolean(reader[3] == DBNull.Value ? 0 : reader[3]),
                            discount = Convert.ToBoolean(reader[4] == DBNull.Value ? 0 : reader[4]),
                        });
                    }
                }
                else
                {
                }
                reader.Close();


            }
            connection.Close();

            return settingPermission;

        }


        [HttpGet]
        public dynamic DefinitionLookupPermission()
        {
            List<PermissionLookupViewModel> list = new List<PermissionLookupViewModel>();
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@"select id, description from  lkp_permission plp  ", connection))
            {
                cmd.Prepare();
                NpgsqlDataReader reader = cmd.ExecuteReader();


                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        list.Add(new PermissionLookupViewModel
                        {
                            id = Convert.ToDouble(reader[0]),
                            description = reader[1]?.ToString(),
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
        public dynamic DefinitionLookupStore()
        {
            List<StoreLookupViewModel> list = new List<StoreLookupViewModel>();
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@"select id,description from  store ps ", connection))
            {
                cmd.Prepare();
                NpgsqlDataReader reader = cmd.ExecuteReader();


                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        list.Add(new StoreLookupViewModel
                        {
                            id = Convert.ToDouble(reader[0]),
                            description = reader[1]?.ToString(),
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
        public dynamic DefinitionLookupStoreGroup()
        {
            List<StoreGroupLookupViewModel> list = new List<StoreGroupLookupViewModel>();
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@"select id,description from  store_group ps ", connection))
            {
                cmd.Prepare();
                NpgsqlDataReader reader = cmd.ExecuteReader();


                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        list.Add(new StoreGroupLookupViewModel
                        {
                            id = Convert.ToDouble(reader[0]),
                            description = reader[1]?.ToString(),
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
        public dynamic DefinitionAccessibilityList()
        {
            List<DefinitionAccessibilityViewModel> definitionAccessibility = new List<DefinitionAccessibilityViewModel>();
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@"select 
                                                da.personnelcode,
                                                ps.description ,
                                                pp.userpermission ,
                                                documentary,
                                                category ,
                                                personnel ,
                                                store ,
                                                product 
                                                from definition_accessibility da 
                                                join  personnel pp on pp.personnelcode = da.personnelcode
                                                left join  store ps on ps.id = pp.storeid", connection))
            {
                cmd.Prepare();
                NpgsqlDataReader reader = cmd.ExecuteReader();


                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        definitionAccessibility.Add(new DefinitionAccessibilityViewModel
                        {
                            personnelCode = reader[0]?.ToString(),
                            storeName = reader[1]?.ToString() == "" ? "GM" : reader[1]?.ToString(),
                            permission = reader[2]?.ToString(),
                            document = Convert.ToBoolean(reader[3] == DBNull.Value ? 0 : reader[3]),
                            category = Convert.ToBoolean(reader[4] == DBNull.Value ? 0 : reader[4]),
                            personnel = Convert.ToBoolean(reader[5] == DBNull.Value ? 0 : reader[5]),
                            store = Convert.ToBoolean(reader[6] == DBNull.Value ? 0 : reader[6]),
                            product = Convert.ToBoolean(reader[7] == DBNull.Value ? 0 : reader[7]),
                        });
                    }
                }
                else
                {
                }
                reader.Close();


            }
            connection.Close();

            return definitionAccessibility;

        }


        [HttpGet]
        public dynamic DefinitionPermission(string personnelCode)
        {
            List<DefinitionPermissionViewModel> definitionPermission = new List<DefinitionPermissionViewModel>();
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@"select 
                                                    personnelcode ,
                                                    documentary ,
                                                    category ,
                                                    personnel ,
                                                    store 
                                                    from definition_accessibility da 
                                                    where personnelcode = :personnelCode ", connection))
            {
                cmd.Parameters.AddWithValue("personnelCode", personnelCode);
                cmd.Prepare();
                NpgsqlDataReader reader = cmd.ExecuteReader();


                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        definitionPermission.Add(new DefinitionPermissionViewModel
                        {
                            personnelCode = reader[0]?.ToString(),
                            documentary = Convert.ToBoolean(reader[1] == DBNull.Value ? 0 : reader[1]),
                            category = Convert.ToBoolean(reader[2] == DBNull.Value ? 0 : reader[2]),
                            personnel = Convert.ToBoolean(reader[3] == DBNull.Value ? 0 : reader[3]),
                            store = Convert.ToBoolean(reader[4] == DBNull.Value ? 0 : reader[4]),
                        });
                    }
                }
                else
                {
                }
                reader.Close();


            }
            connection.Close();

            return definitionPermission;

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
        public List<PersonnelViewModel> GetPersonnel()
        {
            List<PersonnelViewModel> list = new List<PersonnelViewModel>();

            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@"select 
                                                personnelcode,
                                                personnelname,
                                                ps.description ,
                                                createdon  as hiredate,
                                                pp.userpermission,
                                                plv.sid 
                                                from  personnel pp
                                                join  store ps on ps.id = pp.storeid
                                                left join  lkp_verifycode plv on plv.id  = pp.verifyid   ", connection))
            {
                cmd.Prepare();
                NpgsqlDataReader reader = cmd.ExecuteReader();


                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        list.Add(new PersonnelViewModel
                        {
                            personnelCode = reader[0].ToString(),
                            personnelName = reader[1].ToString(),
                            storeName = reader[2].ToString(),
                            hireDate = Convert.ToDateTime(reader[3]),
                            permission = reader[4].ToString(),
                            factorId = reader[5].ToString(),
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
        public List<DepartmentViewModel> GetDepartmentDatasource()
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
        public List<CategoryViewModel> GetCategoryDatasource()
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
        public List<StoreViewModel> GetStoreDatasource()
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
        public List<StoreViewModel> GetStore()
        {
            List<StoreViewModel> list = new List<StoreViewModel>();

            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@"select 
                                                pd.id id ,
                                                pd.description description ,
                                                pd.website website,
                                                pd.phonenumber phonenumber ,
                                                psg.id ,
                                                pd.taxrate
                                                from  store pd
                                                join  store_group psg on psg.id = pd.storegroupid ", connection))
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
                            website = reader[2].ToString(),
                            phoneNumber = reader[3].ToString(),
                            storeGroupId = Convert.ToInt16(reader[4]),
                            taxRate = Convert.ToDouble(reader[5]),
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
        public dynamic GetInventoryList()
        {
            List<DefinitionInventoryViewModel> list = new List<DefinitionInventoryViewModel>();
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

            using (var cmd = new NpgsqlCommand(@"select distinct pp.id,title,department, pd.id as departmantid ,category, pc.id as categoryid ,instock,inrepair,pp.storeid,ps.description ,ebayprice,amazonprice,sellprice 
													from vw_product pp 
                                                    left join  department pd on pd.description = pp.department 
                                                    left join  category pc on pc.description = pp.category 
                                                    left join  store ps  on ps.id = pp.storeid", connection))
            {
                //cmd.Prepare();
                NpgsqlDataReader reader = cmd.ExecuteReader();


                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        list.Add(new DefinitionInventoryViewModel
                        {
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
                            sellPrice = Convert.ToDouble(reader[12] == DBNull.Value ? 0 : reader[12])
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

        [HttpPost]
        [Consumes("application/x-www-form-urlencoded")]
        public dynamic AddReportAccesibility([FromForm] string values)
        {

            //log table
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();
            var reportAccesibilityValue = JsonConvert.DeserializeObject<ReportAccessibilityViewModel>(values);

            using (var cmd = new NpgsqlCommand(@"insert into report_accessibility (
                                                    personnelcode ,
                                                    store,
                                                    cashier ,
                                                    invoice ,
                                                    customer ,
                                                    stock ,
                                                    salesanalysis ,
                                                    inventoryorder ,
                                                    undercount ,
                                                    productrequest 
                                                    )
                                                    values(
                                                    :personnelCode ,
                                                    :store,
                                                    :cashier ,
                                                    :invoice ,
                                                    :customer ,
                                                    :stock ,
                                                    :salesAnalysis ,
                                                    :inventoryOrder ,
                                                    :undercount ,
                                                    :productRequest 
                                                    )", connection))
            {
                cmd.Parameters.AddWithValue("personnelCode", reportAccesibilityValue.personnelCode);
                cmd.Parameters.AddWithValue("store", reportAccesibilityValue.store == null ? false : reportAccesibilityValue.store);
                cmd.Parameters.AddWithValue("cashier", reportAccesibilityValue.cashier == null ? false : reportAccesibilityValue.cashier);
                cmd.Parameters.AddWithValue("invoice", reportAccesibilityValue.invoice == null ? false : reportAccesibilityValue.invoice);
                cmd.Parameters.AddWithValue("customer", reportAccesibilityValue.customer == null ? false : reportAccesibilityValue.customer);
                cmd.Parameters.AddWithValue("stock", reportAccesibilityValue.stock == null ? false : reportAccesibilityValue.stock);
                cmd.Parameters.AddWithValue("salesAnalysis", reportAccesibilityValue.salesAnalysis == null ? false : reportAccesibilityValue.salesAnalysis);
                cmd.Parameters.AddWithValue("inventoryOrder", reportAccesibilityValue.inventoryOrder == null ? false : reportAccesibilityValue.inventoryOrder);
                cmd.Parameters.AddWithValue("undercount", reportAccesibilityValue.undercount == null ? false : reportAccesibilityValue.undercount);
                cmd.Parameters.AddWithValue("productRequest", reportAccesibilityValue.productRequest == null ? false : reportAccesibilityValue.productRequest);
                cmd.ExecuteNonQuery();

            }
            connection.Close();
            return true;
        }

        [HttpPost]
        [Consumes("application/x-www-form-urlencoded")]
        public dynamic AddDefinitionAccesibility([FromForm] string values)
        {

            //log table
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();
            var definitionAccesibilityValue = JsonConvert.DeserializeObject<DefinitionAccessibilityViewModel>(values);

            using (var cmd = new NpgsqlCommand(@"insert into definition_accessibility  (
                                                    personnelcode ,
                                                    documentary,
                                                    category ,
                                                    personnel ,
                                                    store ,
                                                    product
                                                    )
                                                    values(
                                                    :personnelCode ,
                                                    :documentary,
                                                    :category ,
                                                    :personnel ,
                                                    :store ,
                                                    :product 
                                                    )", connection))
            {
                cmd.Parameters.AddWithValue("personnelCode", definitionAccesibilityValue.personnelCode);
                cmd.Parameters.AddWithValue("documentary", definitionAccesibilityValue.document == null ? false : definitionAccesibilityValue.document);
                cmd.Parameters.AddWithValue("category", definitionAccesibilityValue.category == null ? false : definitionAccesibilityValue.category);
                cmd.Parameters.AddWithValue("personnel", definitionAccesibilityValue.personnel == null ? false : definitionAccesibilityValue.personnel);
                cmd.Parameters.AddWithValue("store", definitionAccesibilityValue.store == null ? false : definitionAccesibilityValue.store);
                cmd.Parameters.AddWithValue("product", definitionAccesibilityValue.product == null ? false : definitionAccesibilityValue.product);
                cmd.ExecuteNonQuery();

            }
            connection.Close();
            return true;
        }

        [HttpPost]
        [Consumes("application/x-www-form-urlencoded")]
        public dynamic AddSettingAccesibility([FromForm] string values)
        {

            //log table
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();
            var settingAccesibilityValue = JsonConvert.DeserializeObject<SettingAccessibilityViewModel>(values);

            using (var cmd = new NpgsqlCommand(@"insert into setting_accessibility  (
                                                    personnelcode ,
                                                    price,
                                                    inventoryorder  ,
                                                    promotions 
                                                    )
                                                    values(
                                                    :personnelCode ,
                                                    :price,
                                                    :inventoryOrder ,
                                                    :promotions 
                                                    )", connection))
            {
                cmd.Parameters.AddWithValue("personnelCode", settingAccesibilityValue.personnelCode);
                cmd.Parameters.AddWithValue("price", settingAccesibilityValue.price == null ? false : settingAccesibilityValue.price);
                cmd.Parameters.AddWithValue("inventoryOrder", settingAccesibilityValue.inventoryOrder == null ? false : settingAccesibilityValue.inventoryOrder);
                cmd.Parameters.AddWithValue("promotions", settingAccesibilityValue.promotions == null ? false : settingAccesibilityValue.promotions);
                cmd.ExecuteNonQuery();

            }
            connection.Close();
            return true;
        }

        [HttpPost]
        [Consumes("application/x-www-form-urlencoded")]
        public dynamic AddDepartment([FromForm] string values)
        {

            //log table
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();
            var departmentValue = JsonConvert.DeserializeObject<DefinitionViewModel>(values);

            using (var cmd = new NpgsqlCommand(@" insert into  department (description) values(:description)", connection))
            {
                cmd.Parameters.AddWithValue("description", departmentValue?.description?.ToString() == null ? 0 : departmentValue.description);
                cmd.ExecuteNonQuery();

            }
            connection.Close();
            return true;
        }

        [HttpPost]
        [Consumes("application/x-www-form-urlencoded")]
        public dynamic AddCategory([FromForm] string values)
        {

            //log table
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();
            var categoryValue = JsonConvert.DeserializeObject<DefinitionViewModel>(values);

            using (var cmd = new NpgsqlCommand(@" insert into  category (description) values(:description)", connection))
            {
                cmd.Parameters.AddWithValue("description", categoryValue?.description?.ToString());
                cmd.ExecuteNonQuery();

            }
            connection.Close();
            return true;
        }

        [HttpPost]
        [Consumes("application/x-www-form-urlencoded")]
        public dynamic AddPersonnel([FromForm] string values)
        {

            //log table
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();
            var personnelValue = JsonConvert.DeserializeObject<PersonnelViewModel>(values);

            using (var cmd = new NpgsqlCommand(@" insert into  personnel (
                                                    personnelCode,personnelname,storeid,createdon,userpermission,verifyid) 
                                                    values(:personnelCode,:personnelName,
                                                            (select id from  store where description = :store),
                                                           :createdOn,:permission,
                                                            (select id from  lkp_verifycode plv where 
                                                            (case when (select verifyid from  personnel pp left join  lkp_verifycode plv on plv.id = pp.verifyid 
					                                                            where pp.storeid = (select id from  store where description = :store) and plv.id is not null limit 1)  is null 
                                                            then id is not null
                                                            else id >  (select verifyid  from  personnel pp left join  lkp_verifycode plv on plv.id = pp.verifyid 
			                                                              where pp.storeid = (select id from  store where description = :store) and plv.id is not null
			                                                              order by verifyid desc limit 1 )
			                                                              end)
			                                                              limit 1)
                                                    )", connection))
            {
                cmd.Parameters.AddWithValue("personnelCode", personnelValue.personnelCode);
                cmd.Parameters.AddWithValue("personnelName", personnelValue.personnelName);
                cmd.Parameters.AddWithValue("store", personnelValue.storeName);
                cmd.Parameters.AddWithValue("permission",personnelValue.permission);
                cmd.Parameters.AddWithValue("createdOn", personnelValue.hireDate.ToString() == "1/1/0001 12:00:00 AM" ? DateTime.Now : personnelValue.hireDate);
                cmd.ExecuteNonQuery();

            }
            connection.Close();
            return true;
        }


        [HttpPost]
        [Consumes("application/x-www-form-urlencoded")]
        public dynamic AddStore([FromForm] string values)
        {

            //log table
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();
            var storeValue = JsonConvert.DeserializeObject<DefinitionViewModel>(values);

            using (var cmd = new NpgsqlCommand(@" insert into  store (description,website,phonenumber,storegroupid,taxrate) values(:description,:website,:phoneNumber,:storeGroupId,:taxRate)", connection))
            {
                cmd.Parameters.AddWithValue("description", storeValue.description == null ? DBNull.Value : storeValue.description);
                cmd.Parameters.AddWithValue("website", storeValue.website == null ? DBNull.Value : storeValue.website);
                cmd.Parameters.AddWithValue("phoneNumber", storeValue.phoneNumber == null ? DBNull.Value : storeValue.phoneNumber);
                cmd.Parameters.AddWithValue("storeGroupId", storeValue.storeGroupId == null ? DBNull.Value : storeValue.storeGroupId);
                cmd.Parameters.AddWithValue("taxRate", storeValue.taxRate == null ? 8 : storeValue.taxRate);
                cmd.ExecuteNonQuery();

            }
            connection.Close();
            return true;
        }


        [HttpPost]
        [Consumes("application/x-www-form-urlencoded")]
        public dynamic AddPriceRate([FromForm] string values)
        {

            //log table
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();
            var priceRateValue = JsonConvert.DeserializeObject<SettingPriceViewModel>(values);

            using (var cmd = new NpgsqlCommand(@"INSERT INTO  updateprice (category, sellprice, ebayprice, amazonprice, createdon,updatedon)
                                              values(
                                              :category,
                                              :sellPrice,
                                              :ebayPrice,
                                              :amazonPrice,
                                              current_timestamp,
                                              current_timestamp)
                                            ON CONFLICT(category)
                                            DO UPDATE 
                                            SET
  	                                            sellprice = :sellPrice ,
												ebayprice  = :ebayPrice  ,
												amazonprice  = :amazonPrice  ,
  	                                            updatedon = current_timestamp", connection))
            {
                cmd.Parameters.AddWithValue("category", priceRateValue.category);
                cmd.Parameters.AddWithValue("sellPrice", priceRateValue.sellPrice == null ? 0 : priceRateValue.sellPrice);
                cmd.Parameters.AddWithValue("ebayPrice", priceRateValue.ebayPrice == null ? 0 : priceRateValue.ebayPrice);
                cmd.Parameters.AddWithValue("amazonPrice", priceRateValue.amazonPrice == null ? 0 : priceRateValue.amazonPrice);
                cmd.ExecuteNonQuery();

            }
            using (var cmd = new NpgsqlCommand(@"update  pricecharting 
                                                    set 
                                                    cibprice = cibprice  + cibprice * :sellPrice / 100,
                                                    ebayprice  = ebayprice  + ebayprice * :ebayPrice / 100,
                                                    amazonprice  = amazonprice  + amazonprice * :amazonPrice / 100
                                                    where category =  (select description from  category pc  where id = :category) ", connection))
            {
                cmd.Parameters.AddWithValue("category", Convert.ToDouble(priceRateValue.category));
                cmd.Parameters.AddWithValue("sellPrice", priceRateValue.sellPrice == null ? 0 : priceRateValue.sellPrice);
                cmd.Parameters.AddWithValue("ebayPrice", priceRateValue.ebayPrice == null ? 0 : priceRateValue.ebayPrice);
                cmd.Parameters.AddWithValue("amazonPrice", priceRateValue.amazonPrice == null ? 0 : priceRateValue.amazonPrice);
                cmd.ExecuteNonQuery();

            }
            connection.Close();
            return true;
        }


        [HttpPut]
        [Consumes("application/x-www-form-urlencoded")]
        public dynamic UpdateReportAccesibility([FromForm] string values, string key)
        {

            //log table
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();
            var accessibilityReport = JsonConvert.DeserializeObject<ReportAccessibilityViewModel>(values);

            using (var cmd = new NpgsqlCommand(@"update report_accessibility
                                                        set
                                                        store            =  :store ,        
                                                        cashier          =  :cashier     ,  
                                                        invoice          =  :invoice       ,
                                                        customer         =  :customer      ,
                                                        stock            =  :stock         ,
                                                        salesanalysis    =  :salesAnalysis ,
                                                        inventoryorder   =  :inventoryOrder,
                                                        undercount       =  :undercount    ,
                                                        productrequest   =  :productRequest
                                                        where personnelcode = :personnelCode
                                                        ", connection))
            {
                cmd.Parameters.AddWithValue("store", accessibilityReport.store == null ? false : true);
                cmd.Parameters.AddWithValue("cashier", accessibilityReport.cashier == null ? false : true);
                cmd.Parameters.AddWithValue("invoice", accessibilityReport.invoice == null ? false : true);
                cmd.Parameters.AddWithValue("customer", accessibilityReport.customer == null ? false : true);
                cmd.Parameters.AddWithValue("stock", accessibilityReport.stock == null ? false : true);
                cmd.Parameters.AddWithValue("salesAnalysis", accessibilityReport.salesAnalysis == null ? false : true);
                cmd.Parameters.AddWithValue("inventoryOrder", accessibilityReport.inventoryOrder == null ? false : true);
                cmd.Parameters.AddWithValue("undercount", accessibilityReport.undercount == null ? false : true);
                cmd.Parameters.AddWithValue("productRequest", accessibilityReport.productRequest == null ? false : true);
                cmd.Parameters.AddWithValue("personnelCode", key);
                cmd.ExecuteNonQuery();

            }
            connection.Close();
            return true;
        }

        [HttpPut]
        [Consumes("application/x-www-form-urlencoded")]
        public dynamic SettingsWeeklyReport([FromForm] string values, string key)
        {

            //log table
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();
            var weeklyReport = JsonConvert.DeserializeObject<SettingWeeklyInventoryViewModel>(values);
            var weeklyReportKeys = JsonConvert.DeserializeObject<SettingWeeklyInventoryViewModel>(key);

            using (var cmd = new NpgsqlCommand(@"insert into  weekly_report (itemid,isselected,itemorder,storeid,createdon)
                                                    values(:itemId,:isSelected,:itemOrder,(select id from  store  where description = :storeName),current_timestamp)
                                                    ON CONFLICT (itemid,storeid) 
                                                    DO UPDATE SET
                                                    isselected = :isSelected ,
                                                    itemorder = :itemOrder ,
                                                    updatedon = current_timestamp", connection))
            {
                cmd.Parameters.AddWithValue("isSelected", weeklyReport.isSelected != false || weeklyReport.itemOrder > 0 ? true : false);
                cmd.Parameters.AddWithValue("itemOrder", weeklyReport.isSelected == true && weeklyReport.itemOrder == null ? 1 : (weeklyReport.itemOrder == null ? 1 : weeklyReport.itemOrder));
                cmd.Parameters.AddWithValue("storeName", weeklyReportKeys.storeName);
                cmd.Parameters.AddWithValue("itemId", weeklyReportKeys.id);
                cmd.ExecuteNonQuery();

            }
            connection.Close();
            return true;
        }

        [HttpPost]
        [Consumes("application/x-www-form-urlencoded")]
        public dynamic SettingsPromotionReport([FromForm] string values)
        {

            //log table
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();
            var promotionReport = JsonConvert.DeserializeObject<SettingPromotionViewModel>(values);

            using (var cmd = new NpgsqlCommand(@"insert into  promotion  (promotionname,startdate,enddate,createdon)
                                                    values(:promotionName,:startDate,:endDate,current_timestamp)
                                                    ON CONFLICT (id) 
                                                    DO UPDATE SET
                                                    promotionname = :promotionName ,
                                                    startdate = :startDate ,
                                                    enddate = :endDate ,
                                                    updatedon = current_timestamp", connection))
            {
                cmd.Parameters.AddWithValue("promotionName", promotionReport.promotionName);
                cmd.Parameters.AddWithValue("startDate", promotionReport.startDate );
                cmd.Parameters.AddWithValue("endDate", promotionReport.endDate);
                cmd.ExecuteNonQuery();

            }
            connection.Close();
            return true;
        }

        [HttpPut]
        [Consumes("application/x-www-form-urlencoded")]
        public dynamic SettingsProductDiscount([FromForm] string values, string key)
        {

            //log table
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();
            var productDiscount= JsonConvert.DeserializeObject<SettingDiscountViewModel>(values);

            using (var cmd = new NpgsqlCommand(@"insert into  product_discount (itemid,
                                                            discountrate ,
                                                            createdon 
                                                            )
                                                            values(:itemId, :discountRate,current_timestamp)
                                                            on conflict (itemid)
                                                            do update set
                                                            discountrate = :discountRate ,
                                                            updatedon = current_timestamp ", connection))
            {
                cmd.Parameters.AddWithValue("discountRate", productDiscount.discountRate);
                cmd.Parameters.AddWithValue("itemId", Convert.ToDouble(key));
                cmd.ExecuteNonQuery();

            }
            connection.Close();
            return true;
        }

        [HttpDelete]
        [Consumes("application/x-www-form-urlencoded")]
        public dynamic SettingsDeletePromotionReport([FromForm] double key)
        {

            //log table
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();


            using (var cmd = new NpgsqlCommand(@"delete from  promotion where id = :id", connection))
            {
                cmd.Parameters.AddWithValue("id", key);
                cmd.ExecuteNonQuery();

            }
            connection.Close();
            return true;
        }

        [HttpDelete]
        [Consumes("application/x-www-form-urlencoded")]
        public dynamic DeletePriceRate([FromForm] double key)
        {

            //log table
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();

           

            using (var cmd = new NpgsqlCommand(@"with data as (select sellprice , ebayprice , amazonprice , pc.description from  updateprice pu
                                                join  category pc on pc.id = pu.category::integer
                                                where pu.id = :id)
                                                update  pricecharting pc
                                                                                                    set 
                                                                                                    cibprice = pc.cibprice  - pc.cibprice * data.sellprice /100,
                                                                                                    ebayprice  = pc.ebayprice  - pc.ebayprice * data.sellprice /100,
                                                                                                    amazonprice  = pc.amazonprice  - pc.amazonprice * data.sellprice /100
                                                from
                                                data
                                                where category = data.description", connection))
            {
                cmd.Parameters.AddWithValue("id", key);
                cmd.ExecuteNonQuery();

            }

            using (var cmd = new NpgsqlCommand(@"delete from  updateprice where id = :id", connection))
            {
                cmd.Parameters.AddWithValue("id", key);
                cmd.ExecuteNonQuery();

            }
            connection.Close();
            return true;
        }

        [HttpDelete]
        [Consumes("application/x-www-form-urlencoded")]
        public dynamic DeleteDepartment([FromForm] double key)
        {

            //log table
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();


            using (var cmd = new NpgsqlCommand(@"delete from  department where id = :id", connection))
            {
                cmd.Parameters.AddWithValue("id", key);
                cmd.ExecuteNonQuery();

            }
            connection.Close();
            return true;
        }

        [HttpDelete]
        [Consumes("application/x-www-form-urlencoded")]
        public dynamic DeleteCategory([FromForm] double key)
        {

            //log table
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();


            using (var cmd = new NpgsqlCommand(@"delete from  category where id = :id", connection))
            {
                cmd.Parameters.AddWithValue("id", key);
                cmd.ExecuteNonQuery();

            }
            connection.Close();
            return true;
        }

        [HttpDelete]
        [Consumes("application/x-www-form-urlencoded")]
        public dynamic DeletePersonnel([FromForm] double key)
        {

            //log table
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();


            using (var cmd = new NpgsqlCommand(@"delete from  personnel where id = :id", connection))
            {
                cmd.Parameters.AddWithValue("id", key);
                cmd.ExecuteNonQuery();

            }
            connection.Close();
            return true;
        }

        [HttpDelete]
        [Consumes("application/x-www-form-urlencoded")]
        public dynamic DeleteStore([FromForm] double key)
        {

            //log table
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();


            using (var cmd = new NpgsqlCommand(@"delete from  store where id = :id", connection))
            {
                cmd.Parameters.AddWithValue("id", key);
                cmd.ExecuteNonQuery();

            }
            connection.Close();
            return true;
        }

        [HttpDelete]
        [Consumes("application/x-www-form-urlencoded")]
        public dynamic DeleteUndercount([FromForm] double key)
        {

            //log table
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();


            using (var cmd = new NpgsqlCommand(@"delete from  undercount where id = :id", connection))
            {
                cmd.Parameters.AddWithValue("id", key);
                cmd.ExecuteNonQuery();

            }
            connection.Close();
            return true;
        }

        [HttpDelete]
        [Consumes("application/x-www-form-urlencoded")]
        public dynamic DeleteDefinitionAccessibility([FromForm] string key)
        {

            //log table
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();


            using (var cmd = new NpgsqlCommand(@"delete from definition_accessibility where personnelcode = :personnelCode", connection))
            {
                cmd.Parameters.AddWithValue("personnelCode", key);
                cmd.ExecuteNonQuery();

            }
            connection.Close();
            return true;
        }

        [HttpDelete]
        [Consumes("application/x-www-form-urlencoded")]
        public dynamic DeleteReportAccessibility([FromForm] string key)
        {

            //log table
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();


            using (var cmd = new NpgsqlCommand(@"delete from report_accessibility where personnelcode = :personnelCode", connection))
            {
                cmd.Parameters.AddWithValue("personnelCode", key);
                cmd.ExecuteNonQuery();

            }
            connection.Close();
            return true;
        }

        [HttpDelete]
        [Consumes("application/x-www-form-urlencoded")]
        public dynamic DeleteSettingAccessibility([FromForm] string key)
        {

            //log table
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();


            using (var cmd = new NpgsqlCommand(@"delete from setting_accessibility where personnelcode = :personnelCode", connection))
            {
                cmd.Parameters.AddWithValue("personnelCode", key);
                cmd.ExecuteNonQuery();

            }
            connection.Close();
            return true;
        }

        [HttpPost]
        public dynamic SettingsUpdatePrice(double? sellPriceRate, double? ebayPriceRate, double? amazonPriceRate, string category)
        {
            //log table
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();


            using (var cmd = new NpgsqlCommand(@" update  pricecharting 
                                                    set 
                                                    newcibprice = cibprice  + cibprice * :sellPriceRate / 100,
                                                    newebayprice  = ebayprice  + ebayprice * :ebayPriceRate / 100,
                                                    newamazonprice  = amazonprice  + amazonprice * :amazonPriceRate / 100
                                                    where category = :category ", connection))
            {
                cmd.Parameters.AddWithValue("sellPriceRate", sellPriceRate);
                cmd.Parameters.AddWithValue("ebayPriceRate", ebayPriceRate);
                cmd.Parameters.AddWithValue("amazonPriceRate", amazonPriceRate);
                cmd.Parameters.AddWithValue("category", category);
                cmd.ExecuteNonQuery();

            }
            connection.Close();
            return true;
        }

        [HttpPost]
        public dynamic AdminPermission(string cashierId)
        {

            List<ValidationViewModel> list = new List<ValidationViewModel>();
            //log table
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();
            var response = "";
            using (var cmd = new NpgsqlCommand(@"  select userpermission 
                                                    from  personnel pp
                                                    where personnelcode  = :personnelCode and userpermission  != 'Manager'", connection))
            {
                cmd.Parameters.AddWithValue("personnelCode", cashierId);

                response = cmd.ExecuteScalar()?.ToString();



            };

            connection.Close();


            return response == null ? false : true;
        }




    }
}