using Concur.Core;
using CESMidtier = Concur.Spend.CESMidtier;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

//See below for available fields
//C:\Expense\emt\midtier-web\src\main\java\com\concur\midtier\webservices\xmlhttp\actions\SmartExpenses\GetSmartExpenses.java

namespace Snowbird
{
    public class SmartExpenseEmt
    {
        [DataMember(EmitDefaultValue = false)]
        public string MeKey { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string ExpKey { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string ExpName { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string LocName { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string VendorDescription { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string Comment { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public decimal TransactionAmount { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string CrnCode { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public DateTime TransactionDate { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public byte[] ReceiptImage { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string MobileReceiptImageId { get; set; }
        [DataMember(EmitDefaultValue = false)]

        public string PctKey { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string CctKey { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public object EntryDetails { get; set; }

        public static MobileEntry ToMobileEntry(SmartExpenseEmt v, OTProtect prot)
        {
            return new MobileEntry()
            {
                MeKey = prot.ProtectNonBlankValue("MeKey", v.MeKey),
                ExpKey = v.ExpKey,
                ExpName = v.ExpName,
                LocationName = v.LocName,
                VendorName = v.VendorDescription,
                Comment = v.Comment,
                TransactionAmount = v.TransactionAmount,
                CrnCode = v.CrnCode,
                TransactionDate = v.TransactionDate,
                ReceiptImage = v.ReceiptImage,
                ReceiptImageId = prot.ProtectNonBlankValue(ReceiptStore.RECEIPT_IMAGE_ID_PROTECT_KEY, v.MobileReceiptImageId),
								ReceiptImageIdUnprotected = v.MobileReceiptImageId,
								HasReceiptImage = v.MobileReceiptImageId != null ? "Y" : "N",
                PctKey = prot.ProtectNonBlankValue("PctKey", v.PctKey),
                CctKey = prot.ProtectNonBlankValue("CctKey", v.CctKey),
                MileageDetails = (v.EntryDetails == null || v.EntryDetails.ToString() == "") ?
                    null : MobileEntryUtils.GetMileageDetails(v.EntryDetails.ToString()),
                MobileEntryFormFields = null
            };   
        }     
    }

    public class SmartExpenseEmtUtils
    {
        public static bool UseEMTForGetMobileEntry()
        {
            return QuickExpenseV4Utils.UseQuickExpenseService();
        }
        private static XmlDocument CreateEmtRequest(
            string expenseCompanyId,
            string langCode,
            string empKey,
            string userId)
        {
            StringBuilder sb = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(sb, new XmlWriterSettings() { Indent = false, OmitXmlDeclaration = true }))
            {
                writer.WriteStartElement("GetSmartExpenses");
                writer.WriteElementString("LangCode", langCode);
                writer.WriteElementString("CompanyId", expenseCompanyId);
                writer.WriteElementString("UserId", userId);
                writer.WriteElementString("EditedForEmpKey", empKey);
                writer.WriteElementString("EditedByEmpKey", empKey);
                writer.WriteElementString("Reset", "Y");    // This stops EMT giving us the cached/out-of-date data
                writer.WriteElementString("Start", "0");
                writer.WriteElementString("Limit", "1000");
                writer.Close();

                XmlDocument xmlRequest = new XmlDocument();
                xmlRequest.LoadXml(sb.ToString());

                return CESMidtier.WrapMidtierRequest(xmlRequest);
            }
        }
        public static MobileEntry[] GetMobileEntriesFromEmt(
            OTProtect prot,
            string expenseCompanyId,
            string expenseEntityID,
            string langCode,
            long empKey,
            string userId)
        {

            XmlDocument request = CreateEmtRequest(expenseCompanyId, langCode, empKey.ToString(), userId);
            XmlDocument response = CESMidtier.MakeExpenseMidtierRequest("GetSmartExpenses", request, 60, expenseEntityID); // new
            return GetMobileEntriesFromResponse(prot, response);
        }

        private static MobileEntry[] GetMobileEntriesFromResponse(OTProtect prot, XmlDocument response)
        {
            XmlNode totalNode = response.SelectSingleNode("Response/Body/Total");

            if (totalNode != null)
            {
                int count = Convert.ToInt32(totalNode.InnerText);
                MobileEntry[] entries = new MobileEntry[count];
                if (count > 0)
                {
                    XmlNodeList nl = response.SelectNodes("Response/Body/SmartExpenses/SmartExpense");
                    int c = 0;
                    foreach (XmlNode n in nl)
                    {
                        string jsonText = JsonConvert.SerializeXmlNode(n);
                        SmartExpenseEmtContainer se = JsonConvert.DeserializeObject<SmartExpenseEmtContainer>(jsonText);
                        entries[c++] = SmartExpenseEmt.ToMobileEntry(se.SmartExpense, prot);
                    }
                   
                }
                return entries;
            }
            return new MobileEntry[] { };
        }

        //Used during deserialization
        class SmartExpenseEmtContainer
        {
            [DataMember(EmitDefaultValue = false)]
            public SmartExpenseEmt SmartExpense { get; set; }
        }
    }
}

/*
+    <Request>
+      <Header>
+        <Log>
+          <Level>None</Level>
+        </Log>
+        <EntityID>#{:xmerl_lib.export_text(entity_id)}</EntityID>
+        </Header>
+        <Body>
+          <Action>
+            <GetSmartExpenses>
+              <EditedForEmpKey>#{:xmerl_lib.export_text(emp_key)}</EditedForEmpKey>
+              <EditedByEmpKey>#{:xmerl_lib.export_text(emp_key)}</EditedByEmpKey>
+              <LangCode>en</LangCode>
+              <CompanyId>#{context.assigns.raw_profile["com:concur:Employee:1.0"]["companyInternalId"]}</CompanyId>
+              <Reset>reset</Reset>
+              <Start>0</Start>
+              <Limit>1000</Limit>
+              <SortBy>TransactionDate</SortBy>
+              <SortDir>DESC</SortDir>
+            </GetSmartExpenses>
+          </Action>
+        </Body>
+    </Request>
 */
