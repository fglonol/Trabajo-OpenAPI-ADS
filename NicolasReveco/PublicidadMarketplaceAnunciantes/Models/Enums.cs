namespace PublicidadMarketplaceAnunciantes.Models;

/// <summary>Estado de una campaña publicitaria (RF-F2).</summary>
public enum CampaignStatus
{
    Draft,
    Active,
    Paused,
    Completed
}

/// <summary>Tipo de creativo publicitario (RF-F3).</summary>
public enum CreativeType
{
    VideoAd,
    Banner
}

/// <summary>Estado de revisión de políticas de un creativo (RF-F3).</summary>
public enum PolicyReviewStatus
{
    Pending,
    Approved,
    Rejected
}

/// <summary>Ubicación del espacio publicitario en el inventario (RF-F5).</summary>
public enum Placement
{
    PreRoll,
    MidRoll,
    PostRoll,
    Banner
}

/// <summary>Nivel de brand safety de un espacio o decisión de entrega (RF-F5/RF-F6).</summary>
public enum BrandSafetyLevel
{
    Standard,
    Limited,
    Restricted
}

/// <summary>Tipo de evento de medición publicitaria (RF-F7).</summary>
public enum AdEventType
{
    Impression,
    Click,
    Skip
}

/// <summary>Estado de una factura de anunciante (RF-F8).</summary>
public enum InvoiceStatus
{
    Draft,
    Issued,
    Paid,
    Overdue
}
