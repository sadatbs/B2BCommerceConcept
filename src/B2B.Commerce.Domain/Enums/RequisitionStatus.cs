namespace B2B.Commerce.Domain.Enums;

public enum RequisitionStatus
{
    Submitted = 0,  // Created on cart submit. Cart is the draft workspace.
    Approved  = 1,  // Approved by Approver role (or auto-approved within budget)
    Rejected  = 2,  // Rejected by Approver role
    Ordered   = 3   // System set after Order is created from this Requisition
}
