namespace SuperBlazorComponents.Components.SuperLayout;

public class Device
{
	public string UserAgent { get; set; } = null!;
	public string? Name { get; set; } = "notdetected";
	public string? Platform { get; set; }
	public string? Os { get; set; }
	public bool IsMobile { get; set; } = false;
	public int ScreenWidth { get; set; }
	public int ScreenHeight { get; set; }
	public int AvailableWidth { get; set; }
	public int AvailableHeight { get; set; }
	public int WindowInnerWidth { get; set; }
	public int WindowInnerHeight { get; set; }
}