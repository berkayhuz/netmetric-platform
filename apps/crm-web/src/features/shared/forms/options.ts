export const genderOptions = [
  { value: 0, label: "Unknown" },
  { value: 1, label: "Female" },
  { value: 2, label: "Male" },
  { value: 3, label: "Other" },
] as const;

export const customerTypeOptions = [
  { value: 0, label: "Individual" },
  { value: 1, label: "Corporate" },
] as const;

export const companyTypeOptions = [
  { value: 0, label: "Prospect" },
  { value: 1, label: "Customer" },
  { value: 2, label: "Partner" },
  { value: 3, label: "Vendor" },
  { value: 4, label: "Competitor" },
] as const;

export const booleanOptions = [
  { value: "false", label: "No" },
  { value: "true", label: "Yes" },
] as const;

export const addressTypeOptions = [
  { value: 0, label: "Other" },
  { value: 1, label: "Billing" },
  { value: 2, label: "Shipping" },
  { value: 3, label: "Home" },
  { value: 4, label: "Work" },
] as const;

export const leadSourceOptions = [
  { value: 0, label: "Manual" },
  { value: 1, label: "Website" },
  { value: 2, label: "Referral" },
  { value: 3, label: "Campaign" },
  { value: 4, label: "Email" },
  { value: 5, label: "Phone" },
  { value: 6, label: "Social media" },
  { value: 7, label: "Event" },
  { value: 8, label: "Partner" },
  { value: 9, label: "Web form" },
  { value: 10, label: "Landing page" },
  { value: 11, label: "Support inbox" },
  { value: 12, label: "Chat" },
  { value: 13, label: "API" },
  { value: 99, label: "Other" },
] as const;

export const leadStatusOptions = [
  { value: 0, label: "New" },
  { value: 1, label: "Contacted" },
  { value: 2, label: "Qualified" },
  { value: 3, label: "Unqualified" },
  { value: 4, label: "Converted" },
  { value: 5, label: "Won" },
  { value: 6, label: "Lost" },
  { value: 7, label: "Archived" },
  { value: 8, label: "Recycled" },
  { value: 9, label: "Nurturing" },
] as const;

export const priorityOptions = [
  { value: 0, label: "Low" },
  { value: 1, label: "Medium" },
  { value: 2, label: "High" },
  { value: 3, label: "Critical" },
] as const;

export const ticketTypeOptions = [
  { value: 0, label: "Support" },
  { value: 1, label: "Incident" },
  { value: 2, label: "Problem" },
  { value: 3, label: "Question" },
  { value: 4, label: "Task" },
] as const;

export const ticketChannelOptions = [
  { value: 0, label: "Web" },
  { value: 1, label: "Email" },
  { value: 2, label: "Phone" },
  { value: 3, label: "Chat" },
  { value: 4, label: "Social" },
  { value: 5, label: "API" },
] as const;

export const opportunityStageOptions = [
  { value: 0, label: "Prospecting" },
  { value: 1, label: "Qualification" },
  { value: 2, label: "Needs analysis" },
  { value: 3, label: "Proposal" },
  { value: 4, label: "Negotiation" },
  { value: 5, label: "Won" },
  { value: 6, label: "Lost" },
] as const;

export const opportunityStatusOptions = [
  { value: 0, label: "Open" },
  { value: 1, label: "Won" },
  { value: 2, label: "Lost" },
  { value: 3, label: "Cancelled" },
] as const;
