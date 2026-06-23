namespace MonetizacionEcosistemaCreador.Models;

/// <summary>Estado de la evaluación de elegibilidad de monetización (RF-M1).</summary>
public enum EligibilityStatus
{
    NotEvaluated,
    UnderReview,
    Eligible,
    Rejected
}

/// <summary>Tipo de producto de monetización (RF-M2).</summary>
public enum ProductType
{
    Membership,
    SuperThanks,
    Ads
}

/// <summary>Origen del ingreso registrado (RF-M3).</summary>
public enum RevenueSource
{
    Ads,
    Membership,
    Tip
}

/// <summary>Estado de un pago/payout (RF-M5).</summary>
public enum PayoutStatus
{
    Pending,
    Paid,
    Failed
}

/// <summary>Estado contable de un ingreso hasta que queda disponible para payout (RF-M3).</summary>
public enum RevenueStatus
{
    Pending,
    Confirmed,
    Paid
}

/// <summary>Tipo de documento fiscal requerido para poder cobrar (RF-M5).</summary>
public enum FiscalDocumentType
{
    TaxForm,
    Identity,
    BankAccount
}

/// <summary>Estado de revisión de un documento fiscal (RF-M5).</summary>
public enum FiscalDocumentStatus
{
    PendingReview,
    Approved,
    Rejected
}

/// <summary>Nivel al que aplica una activación/desactivación de monetización (RF-M1).</summary>
public enum MonetizationResourceType
{
    Channel,
    Content
}
