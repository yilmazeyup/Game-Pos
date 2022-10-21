using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Net;
using System.Net.Mail;
using static Test_InventoryPage.ViewModels;

namespace Test_InventoryPage.Pages
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class Registration : ControllerBase
    {



        private readonly ILogger<Registration> _logger;

        public Registration(ILogger<Registration> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public dynamic SendMail(string firstName, string surName, string email, string customerPhone)
        {
            //log table

            List<InvoiceHeaderViewModel> invoiceHeaderList = new List<InvoiceHeaderViewModel>();
            List<InvoiceBodyViewModel> invoiceBodyList = new List<InvoiceBodyViewModel>();
            NpgsqlConnection connection = new NpgsqlConnection("Host=host;Username=username;Password=password;Database=database");
            connection.Open();
            //Invoice header is created
            using (var cmd = new NpgsqlCommand(@"select i.invoiceid, personnelid, i.storeid, pp.description , subtotal ,taxtotal ,grandtotal , date(i.createdon)  , psg.id  ,pg.giftcode, ps.description 
                                                                from invoice i 
                                                                join  paymentmethod pp on pp.id = i.paymentmethod 
                                                                join  store ps on ps.id = i.storeid 
                                                                join  store_group psg on psg.id = ps.storegroupid  
                                                                left join  giftcode pg on pg.invoiceid  = i.invoiceid                                                     
                                                                where i.customerphone = :customerPhone::text 
                                                                and pg.isused = false or pg.isused is null
                                                                order by invoiceid desc limit 1 ", connection))
            {
                cmd.Parameters.AddWithValue("customerPhone", customerPhone);
                cmd.Prepare();
                NpgsqlDataReader reader = cmd.ExecuteReader();


                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        invoiceHeaderList.Add(new InvoiceHeaderViewModel
                        {
                            invoiceId = Convert.ToDouble(reader[0] == DBNull.Value ? 0 : reader[0]),
                            personnelId = (reader[1]?.ToString()),
                            storeId = Convert.ToDouble(reader[2] == DBNull.Value ? 0 : reader[2]),
                            paymentMethod = reader[3]?.ToString(),
                            subTotal = Convert.ToDouble(reader[4] == DBNull.Value ? 0 : reader[4]),
                            taxTotal = Convert.ToDouble(reader[5] == DBNull.Value ? 0 : reader[5]),
                            grandTotal = Convert.ToDouble(reader[6] == DBNull.Value ? 0 : reader[6]),
                            invoiceDate = Convert.ToDateTime(reader[7] == DBNull.Value ? 0 : reader[7]),
                            storeGroupId = Convert.ToDouble(reader[8] == DBNull.Value ? 0 : reader[8]),
                            giftCode = reader[9]?.ToString(),
                            storeName = reader[10]?.ToString(),
                        });
                    }
                }
                else
                {
                }
                reader.Close();


            };


            //Invoice details are created
            using (var cmd = new NpgsqlCommand(@"select productid, productname, quantity,sellprice/quantity,sellprice  
                                                            from invoice i 
                                                            where invoiceid = :invoiceId and sellprice > 0
                                                            union
                                                select null, 'Trade In Products', sum(quantity),sum(sellprice)/sum(quantity),sum(sellprice)
                                                            from invoice i 
                                                            where invoiceid = :invoiceId and sellprice < 0 ", connection))
            {
                cmd.Parameters.AddWithValue("customerPhone", customerPhone);
                cmd.Parameters.AddWithValue("invoiceId", Convert.ToDouble(invoiceHeaderList[0].invoiceId));
                cmd.Prepare();
                NpgsqlDataReader reader = cmd.ExecuteReader();


                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        invoiceBodyList.Add(new InvoiceBodyViewModel
                        {
                            productId = Convert.ToDouble(reader[0] == DBNull.Value ? 0 : reader[0]),
                            productName = reader[1]?.ToString(),
                            quantity = Convert.ToDouble(reader[2] == DBNull.Value ? 0 : reader[2]),
                            unitPrice = Convert.ToDouble(reader[3] == DBNull.Value ? 0 : reader[3]),
                            totalPrice = Convert.ToDouble(reader[4] == DBNull.Value ? 0 : reader[4]),
                        });
                    }
                }
                else
                {
                }
                reader.Close();
            };
            //Customer is added or updated
            using (var cmd = new NpgsqlCommand(@"insert into  customer (firstname,lastname,email,phonenumber,createdon)
            													values(:firstName,:lastName,:email,:phoneNumber,current_timestamp)
            													on 
            													conflict (phonenumber , email)
            													do update set 
            													firstname = :firstName,
            													lastname  = :lastName,
            													updatedon = current_timestamp  ", connection))
            {
                cmd.Parameters.AddWithValue("firstName", firstName);
                cmd.Parameters.AddWithValue("lastName", surName);
                cmd.Parameters.AddWithValue("email", email);
                cmd.Parameters.AddWithValue("phoneNumber", customerPhone);
                cmd.Prepare();
                cmd.ExecuteNonQuery();
            };


            var head = @"<!DOCTYPE html>
            <html lang='en'>
            	<head>
            		<meta charset='utf-8' />
            		<meta name='viewport' content='width=device-width, initial-scale=1' />

            		<title>A simple, clean, and responsive HTML invoice template</title>

            		<!-- Favicon -->
            		<link rel='icon' href='./images/favicon.png' type='image/x-icon' />

            		<!-- Invoice styling -->
            		<style>
            			body {
            				font-family: 'Helvetica Neue', 'Helvetica', Helvetica, Arial, sans-serif;
            				text-align: center;
            				color: #777;
            			}

            			body h1 {
            				font-weight: 300;
            				margin-bottom: 0px;
            				padding-bottom: 0px;
            				color: #000;
            			}

            			body h3 {
            				font-weight: 300;
            				margin-top: 10px;
            				margin-bottom: 20px;
            				font-style: italic;
            				color: #555;
            			}

            			body a {
            				color: #06f;
            			}

            			.invoice-box {
            				max-width: 800px;
            				margin: auto;
            				padding: 30px;
            				border: 1px solid #eee;
            				box-shadow: 0 0 10px rgba(0, 0, 0, 0.15);
            				font-size: 16px;
            				line-height: 24px;
            				font-family: 'Helvetica Neue', 'Helvetica', Helvetica, Arial, sans-serif;
            				color: #555;
            			}

            			.invoice-box table {
            				width: 100%;
            				line-height: inherit;
            				text-align: left;
            				border-collapse: collapse;
            			}

            			.invoice-box table td {
            				padding: 5px;
            				vertical-align: top;
            			}

            			.invoice-box table tr td:nth-child(2) {
            				text-align: right;
            			}

            			.invoice-box table tr.top table td {
            				padding-bottom: 20px;
            			}

            			.invoice-box table tr.top table td.title {
            				font-size: 45px;
            				line-height: 45px;
            				color: #333;
            			}

            			.invoice-box table tr.information table td {
            				padding-bottom: 40px;
            			}

            			.invoice-box table tr.heading td {
            				background: #eee;
            				border-bottom: 1px solid #ddd;
            				font-weight: bold;
            			}

            			.invoice-box table tr.details td {
            				padding-bottom: 20px;
            			}

            			.invoice-box table tr.item td {
            				border-bottom: 1px solid #eee;
            			}


            			.invoice-box table tr.itemName td {
            				border-bottom: 1px solid #eee;
            				text-align: left;
            			}

            			.invoice-box table tr.total td:nth-child(2) {
            				border-top: 2px solid #eee;
            				font-weight: bold;
            			}

            			@media only screen and (max-width: 600px) {
            				.invoice-box table tr.top table td {
            					width: 100%;
            					display: block;
            					text-align: center;
            				}

            				.invoice-box table tr.information table td {
            					width: 100%;
            					display: block;
            					text-align: center;
            				}
            			}
            		</style>
            	</head>";

            var first = $@"
            	<body>
            		<h3>{invoiceHeaderList[0].storeName?.ToString()}</h3>
            		<h3>No Return, All Sales Final</h3>


            		<div class='invoice-box'>
            			<table>
            				<tr class='top'>
            					<td colspan='2'>
            						<table>
            							<tr>
            								<td class='title'>
            									<img src='{(invoiceHeaderList[0].storeGroupId == 1 ? "yourUrl/ProductPhotos/GAME_PLATFORM.png" : "https://www.gamebossllc.com/wp-content/uploads/2020/05/Game-Boss-Logo.png")}' alt='Company logo' style='width: 100%; max-width: 300px' />

                                            </td>

            								<td>
            									Invoice #:{invoiceHeaderList[0].invoiceId.ToString()} <br />
                                                Cashier ID:{invoiceHeaderList[0].personnelId?.ToString()} <br />
                                                Station ID:{invoiceHeaderList[0].storeId.ToString()} <br />
                                                Date:{invoiceHeaderList[0].invoiceDate.ToString()} <br />
            								</td>
            							</tr>
            						</table>
            					</td>
            				</tr>

            				<tr class='information'>
            					<td colspan='2'>
            						<table>
            							<tr>
            								<td>
            									ALL SALES FINAL<br />
            									30 Day Warranty on ALL Used Games<br />
            									If you experince any issue you may:<br />
            									Exchange it for item of same name<br />
            									No exchange for damaged/Cracked items<br />
            									BUY OR SELL YOUR VIDEO GAMES<br />
            								</td>

            								<td>
            									{(invoiceHeaderList[0].storeGroupId == 1 ? "GAME PLATFORM" : "GAME BOSS")}<br />
            									(951) 357-0061<br />
            									{(invoiceHeaderList[0].storeGroupId == 1 ? "Gameplatformca@gmail.com" : "gamerboxllc@gmail.com")}
            								</td>
            							</tr>
            						</table>
            					</td>
            				</tr>

            				<tr class='heading'>
            					<td>Payment Method</td>

            				</tr>

            				<tr class='details'>
            					<td>{invoiceHeaderList[0]?.paymentMethod?.ToString()}</td>

            				</tr>

            				<tr class='heading'>
            					<td>Item</td>                    

            					<td>ItemName</td>              

            					<td>Quantity</td>

            					<td>Unity Price</td>

            					<td>Total Price</td>
            				</tr>";

            var second = "";
            invoiceBodyList.ForEach(x =>
            {
                var item = $@"

            					<tr class='item'>
            					<td>{x.productId}</td>

            					<td>{x.productName}</td>

            					<td>{x.quantity}</td>

            					<td>${Math.Round(Convert.ToDecimal(x.unitPrice), 2)}</td>

            					<td>${Math.Round(Convert.ToDecimal(x.totalPrice), 2)}</td>
            				</tr>";

                second = second + item;

            }
            );
            var third = $@"<tr class='total'>
            									<td></td>
            									<td>Sub Total: ${Math.Round(Convert.ToDecimal(invoiceHeaderList[0].subTotal), 2)}</td>
            								</tr>
            								<tr class='total'>
            									<td></td>
            									<td>Tax Total: ${Math.Round(Convert.ToDecimal(invoiceHeaderList[0].taxTotal), 2)}</td>
            								</tr>
            								<tr class='total'>
            									<td></td>
            									<td>Grand Total: ${Math.Round(Convert.ToDecimal(invoiceHeaderList[0].grandTotal), 2)}</td>
            								</tr>								
            								<tr class='total'>
            									<td></td>
            									<td>Gift Code: ${invoiceHeaderList[0]?.giftCode?.ToString()}</td>
            								</tr>
            							</table>
            						</div>
            					</body>
            				</html>";

            var body = head + first + second + third;

            using (var cmd = new NpgsqlCommand(@"update invoice_id 
            														set
            														invoiceoutput = :invoiceOutput 
            														where id  = (select invoiceid  
            														from invoice i
            														where customerphone = :customerPhone  
            														order by createdon desc limit 1)", connection))
            {
                cmd.Parameters.AddWithValue("invoiceOutput", body);
                cmd.Parameters.AddWithValue("customerPhone", customerPhone);
                cmd.Prepare();
                cmd.ExecuteNonQuery();
            };


            connection.Close();

            string address = invoiceHeaderList[0].storeGroupId == 1 ? "Gameplatformca@gmail.com" : "gamerboxllc@gmail.com";
            //string address = "gameplatformca@gmail.com";
            string password = invoiceHeaderList[0].storeGroupId == 1 ? "aojweceayjycxsyz" : "bghklqgwffwgimhg";
            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient();
            mail.From = new MailAddress(address);
            mail.To.Add(email);
            mail.Subject = invoiceHeaderList[0].storeGroupId == 1 ? "GAME PLATFORM" : "GAME BOSS";
            mail.Body = body;
            mail.IsBodyHtml = true;
            SmtpServer.UseDefaultCredentials = false;
            NetworkCredential NetworkCred = new NetworkCredential(address, password);
            SmtpServer.Credentials = NetworkCred;
            SmtpServer.EnableSsl = true;
            SmtpServer.Port = 587;
            SmtpServer.Host = "smtp.gmail.com";
            SmtpServer.Send(mail);




            return true;



        }
    }
}
