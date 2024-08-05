using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPkarta
{
    public class CardImage
    {
        public string data { get; set; }
        public bool success { get; set; }
    }

    public class User
    {
        public bool success { get; set; }
        public Wertyzuser wertyzUser { get; set; }
    }

    public class Wertyzuser
    {
        public bool active { get; set; }
        public Card[] cards { get; set; }
        public int cityId { get; set; }
        public int countryId { get; set; }
        public string email { get; set; }
        public string firstName { get; set; }
        public int globalUserId { get; set; }
        public int id { get; set; }
        public string isicNumber { get; set; }
        public int languageId { get; set; }
        public string largeImage { get; set; }
        public string lastName { get; set; }
        public string login { get; set; }
        public string mediumImage { get; set; }
        public Personalinformation[] personalInformations { get; set; }
        public string phone { get; set; }
        public string postCode { get; set; }
        public string smallImage { get; set; }
        public int status { get; set; }
        public string street { get; set; }
        public int validFrom { get; set; }
        public int validTo { get; set; }
        public object[] users { get; set; }
        public Userrequest[] userRequests { get; set; }
        public Confirmation confirmation { get; set; }
        public bool isRequestProfileEdit { get; set; }
    }

    public class Confirmation
    {
        public int status { get; set; }
        public string note { get; set; }
    }

    public class Card
    {
        public int cardTypeId { get; set; }
        public string cardTypeName { get; set; }
        public int creditLastBalance { get; set; }
        public int creditLastUsingDate { get; set; }
        public int creditValidFrom { get; set; }
        public int creditValidTo { get; set; }
        public string currencySymbol { get; set; }
        public int discountValidFrom { get; set; }
        public int discountValidTo { get; set; }
        public int id { get; set; }
        public bool isActive { get; set; }
        public bool isBlocked { get; set; }
        public bool isValid { get; set; }
        public int nextDiscountValidFrom { get; set; }
        public int nextDiscountValidTo { get; set; }
        public string organizationName { get; set; }
        public string ownerFirstName { get; set; }
        public string ownerLargeImage { get; set; }
        public string ownerLastName { get; set; }
        public int ownerUserId { get; set; }
        public string snr { get; set; }
        public int systemEntityId { get; set; }
        public string template { get; set; }
        public int templateId { get; set; }
        public Ticket[] tickets { get; set; }
        public int validFrom { get; set; }
        public int validTo { get; set; }
        public Identifier[] identifiers { get; set; }
        public bool requestInProgress { get; set; }
    }

    public class Ticket
    {
        public bool active { get; set; }
        public int price { get; set; }
        public int status { get; set; }
        public int tariffCtl { get; set; }
        public int tariffID { get; set; }
        public string tariffName { get; set; }
        public int ticketId { get; set; }
        public string ticketSNR { get; set; }
        public int ticketSNRId { get; set; }
        public int ticketTypeId { get; set; }
        public string ticketTypeName { get; set; }
        public int timeValidityFrom { get; set; }
        public int timeValidityTo { get; set; }
        public int validFrom { get; set; }
        public int validTo { get; set; }
        public Zone[] zones { get; set; }
    }

    public class Zone
    {
        public int zoneId { get; set; }
        public string zoneName { get; set; }
    }

    public class Identifier
    {
        public int cardStatusId { get; set; }
        public string description { get; set; }
        public int identifTypeDefinitionId { get; set; }
        public string identifTypeDefinitionName { get; set; }
        public string identifTypeName { get; set; }
        public string identification { get; set; }
        public int identificationId { get; set; }
        public int identificationType { get; set; }
        public int validFrom { get; set; }
        public int validTo { get; set; }
    }

    public class Personalinformation
    {
        public string information { get; set; }
        public int subtypeId { get; set; }
    }

    public class Userrequest
    {
        public int ConfirmationDate { get; set; }
        public int Id { get; set; }
        public string Metadata { get; set; }
        public string Note { get; set; }
        public int RefToUsr_User { get; set; }
        public int RefToUsr_UserConfirmed { get; set; }
        public int RequestType { get; set; }
        public int Status { get; set; }
        public int ValidFrom { get; set; }
        public int ValidTo { get; set; }
    }
}
