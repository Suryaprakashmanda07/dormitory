using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saas_Dormitory.Models.ResponseDTO
{
    public class SubscriptionPlanCreateDto
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int? MaxProperties { get; set; }
        public int? MaxUsers { get; set; }
        public int Period { get; set; }
    }
    public class SubscriptionPlanUpdateDto
    {
        public int PlanId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int? MaxProperties { get; set; }
        public int? MaxUsers { get; set; }
        public int Period { get; set; }
    }
    public class SubscriptionPlanDetailsModel
    {
        public int PlanId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int? MaxProperties { get; set; }
        public int? MaxUsers { get; set; }
        public bool? Isactive { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Period { get; set; }           // Value (1–4)
        public string PeriodName { get; set; }
    }
    public class SubscriptionPeriodLookupModel
    {
        public int Id { get; set; }
        public string PeriodName { get; set; }
    }
}
