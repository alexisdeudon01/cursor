using UnityEngine;

/// <summary>
/// Centralized color palette for consistent UI styling across the application.
/// All UI components should reference these colors for consistency.
/// </summary>
public static class UIColors
{
    // ============================================
    //   BACKGROUND COLORS
    // ============================================
    
    /// <summary>Main dark background</summary>
    public static readonly Color Background = new Color(0.10f, 0.10f, 0.12f, 1f);
    
    /// <summary>Panel/Card background</summary>
    public static readonly Color Panel = new Color(0.15f, 0.15f, 0.18f, 1f);
    
    /// <summary>Elevated panel (popup, modal)</summary>
    public static readonly Color PanelElevated = new Color(0.18f, 0.18f, 0.22f, 1f);
    
    /// <summary>Input field background</summary>
    public static readonly Color InputBackground = new Color(0.12f, 0.12f, 0.15f, 1f);
    
    /// <summary>List item background</summary>
    public static readonly Color ListItem = new Color(0.20f, 0.20f, 0.24f, 1f);
    
    /// <summary>List item hover</summary>
    public static readonly Color ListItemHover = new Color(0.25f, 0.25f, 0.30f, 1f);
    
    /// <summary>List item selected</summary>
    public static readonly Color ListItemSelected = new Color(0.25f, 0.35f, 0.50f, 1f);
    
    // ============================================
    //   TEXT COLORS
    // ============================================
    
    /// <summary>Primary text (white)</summary>
    public static readonly Color TextPrimary = new Color(1f, 1f, 1f, 1f);
    
    /// <summary>Secondary text (light gray)</summary>
    public static readonly Color TextSecondary = new Color(0.70f, 0.70f, 0.75f, 1f);
    
    /// <summary>Muted text (gray)</summary>
    public static readonly Color TextMuted = new Color(0.50f, 0.50f, 0.55f, 1f);
    
    /// <summary>Disabled text</summary>
    public static readonly Color TextDisabled = new Color(0.40f, 0.40f, 0.45f, 1f);
    
    // ============================================
    //   ACCENT COLORS
    // ============================================
    
    /// <summary>Primary accent (blue)</summary>
    public static readonly Color AccentPrimary = new Color(0.30f, 0.55f, 0.90f, 1f);
    
    /// <summary>Primary accent hover</summary>
    public static readonly Color AccentPrimaryHover = new Color(0.35f, 0.60f, 0.95f, 1f);
    
    /// <summary>Secondary accent (purple)</summary>
    public static readonly Color AccentSecondary = new Color(0.55f, 0.35f, 0.80f, 1f);
    
    /// <summary>Teal accent</summary>
    public static readonly Color AccentTeal = new Color(0.20f, 0.70f, 0.65f, 1f);
    
    // ============================================
    //   STATUS COLORS
    // ============================================
    
    /// <summary>Success (green)</summary>
    public static readonly Color Success = new Color(0.30f, 0.75f, 0.45f, 1f);
    
    /// <summary>Success hover</summary>
    public static readonly Color SuccessHover = new Color(0.35f, 0.80f, 0.50f, 1f);
    
    /// <summary>Success background (subtle)</summary>
    public static readonly Color SuccessBg = new Color(0.30f, 0.75f, 0.45f, 0.15f);
    
    /// <summary>Warning (orange/gold)</summary>
    public static readonly Color Warning = new Color(0.95f, 0.70f, 0.25f, 1f);
    
    /// <summary>Warning background (subtle)</summary>
    public static readonly Color WarningBg = new Color(0.95f, 0.70f, 0.25f, 0.15f);
    
    /// <summary>Error/Danger (red)</summary>
    public static readonly Color Error = new Color(0.85f, 0.30f, 0.30f, 1f);
    
    /// <summary>Error hover</summary>
    public static readonly Color ErrorHover = new Color(0.90f, 0.35f, 0.35f, 1f);
    
    /// <summary>Error background (subtle)</summary>
    public static readonly Color ErrorBg = new Color(0.85f, 0.30f, 0.30f, 0.15f);
    
    /// <summary>Info (cyan)</summary>
    public static readonly Color Info = new Color(0.30f, 0.75f, 0.95f, 1f);
    
    /// <summary>Info background (subtle)</summary>
    public static readonly Color InfoBg = new Color(0.30f, 0.75f, 0.95f, 0.15f);
    
    // ============================================
    //   BORDER COLORS
    // ============================================
    
    /// <summary>Default border</summary>
    public static readonly Color Border = new Color(0.30f, 0.30f, 0.35f, 1f);
    
    /// <summary>Light border</summary>
    public static readonly Color BorderLight = new Color(0.40f, 0.40f, 0.45f, 1f);
    
    /// <summary>Focus border (accent)</summary>
    public static readonly Color BorderFocus = new Color(0.40f, 0.60f, 0.90f, 1f);
    
    /// <summary>Popup border (highlighted)</summary>
    public static readonly Color BorderPopup = new Color(0.40f, 0.50f, 0.70f, 1f);
    
    // ============================================
    //   BUTTON COLORS
    // ============================================
    
    /// <summary>Primary button</summary>
    public static readonly Color ButtonPrimary = new Color(0.30f, 0.55f, 0.85f, 1f);
    
    /// <summary>Primary button hover</summary>
    public static readonly Color ButtonPrimaryHover = new Color(0.35f, 0.60f, 0.90f, 1f);
    
    /// <summary>Secondary button (neutral)</summary>
    public static readonly Color ButtonSecondary = new Color(0.35f, 0.35f, 0.40f, 1f);
    
    /// <summary>Secondary button hover</summary>
    public static readonly Color ButtonSecondaryHover = new Color(0.40f, 0.40f, 0.45f, 1f);
    
    /// <summary>Success button (green)</summary>
    public static readonly Color ButtonSuccess = new Color(0.25f, 0.65f, 0.40f, 1f);
    
    /// <summary>Success button hover</summary>
    public static readonly Color ButtonSuccessHover = new Color(0.30f, 0.70f, 0.45f, 1f);
    
    /// <summary>Danger button (red)</summary>
    public static readonly Color ButtonDanger = new Color(0.75f, 0.30f, 0.30f, 1f);
    
    /// <summary>Danger button hover</summary>
    public static readonly Color ButtonDangerHover = new Color(0.80f, 0.35f, 0.35f, 1f);
    
    /// <summary>Warning button (orange)</summary>
    public static readonly Color ButtonWarning = new Color(0.85f, 0.55f, 0.20f, 1f);
    
    /// <summary>Warning button hover</summary>
    public static readonly Color ButtonWarningHover = new Color(0.90f, 0.60f, 0.25f, 1f);
    
    // ============================================
    //   BADGE / TAG COLORS
    // ============================================
    
    /// <summary>Host badge (gold)</summary>
    public static readonly Color BadgeHost = new Color(1f, 0.80f, 0.30f, 1f);
    
    /// <summary>Host badge background</summary>
    public static readonly Color BadgeHostBg = new Color(1f, 0.80f, 0.30f, 0.20f);
    
    /// <summary>Ready badge (green)</summary>
    public static readonly Color BadgeReady = new Color(0.40f, 0.85f, 0.55f, 1f);
    
    /// <summary>Ready badge background</summary>
    public static readonly Color BadgeReadyBg = new Color(0.40f, 0.85f, 0.55f, 0.20f);
    
    /// <summary>Not ready badge (red)</summary>
    public static readonly Color BadgeNotReady = new Color(0.90f, 0.45f, 0.45f, 1f);
    
    /// <summary>Not ready badge background</summary>
    public static readonly Color BadgeNotReadyBg = new Color(0.90f, 0.45f, 0.45f, 0.20f);
    
    // ============================================
    //   OVERLAY
    // ============================================
    
    /// <summary>Modal overlay (darkened background)</summary>
    public static readonly Color Overlay = new Color(0f, 0f, 0f, 0.75f);
    
    // ============================================
    //   HELPER METHODS
    // ============================================
    
    /// <summary>
    /// Converts Color to CSS hex string
    /// </summary>
    public static string ToHex(Color color)
    {
        return $"#{ColorUtility.ToHtmlStringRGB(color)}";
    }
    
    /// <summary>
    /// Converts Color to CSS rgba string
    /// </summary>
    public static string ToRgba(Color color)
    {
        return $"rgba({(int)(color.r * 255)}, {(int)(color.g * 255)}, {(int)(color.b * 255)}, {color.a:F2})";
    }
}
