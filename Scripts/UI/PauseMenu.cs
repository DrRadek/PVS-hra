using Godot;
using System;
using System.Collections.Generic;

public partial class PauseMenu : Control
{
    private ItemList trajList;
    private ItemList dmgList;
    private Control shipPreview;
    private Sprite2D shipSprite;
    private Control selector;
    private ColorRect selectorRect;
    private Label tooltip;

    private FunctionsManager functionsManager;

    private int selectedTraj = -1;
    private int selectedDmg = -1;

    private Vector2 selectorOffset = Vector2.Zero;
    private bool dragging = false;
    private float renderScale = 0.1f;
    [Export] public float previewScale = 1f;
    private Vector2 originalShipScale = Vector2.One;

    // cached samples for hover detection
    private Vector2[] lastTrajGlobalPoints = null;
    private Vector2[] lastDmgGlobalPoints = null;
    private float[] lastSampleDistances = null;
    private float[] lastTrajValues = null;
    private float[] lastDmgValues = null;
    // store raw function-space damage values (before normalization)
    private float[] lastRawDmgValues = null;

    public override void _Ready()
    {
        // safe node lookups: scene may have been edited/undone, support both with and without background parent
        trajList = GetNodeOrNull<ItemList>("background/LeftPanel/TrajectoryList") ?? GetNodeOrNull<ItemList>("LeftPanel/TrajectoryList");
        dmgList = GetNodeOrNull<ItemList>("background/RightPanel/DamageList") ?? GetNodeOrNull<ItemList>("RightPanel/DamageList");
        shipPreview = GetNodeOrNull<Control>("background/CenterPanel/ShipPreview") ?? GetNodeOrNull<Control>("CenterPanel/ShipPreview");
        shipSprite = GetNodeOrNull<Sprite2D>("background/CenterPanel/ShipPreview/ShipSprite") ?? GetNodeOrNull<Sprite2D>("CenterPanel/ShipPreview/ShipSprite");
        selector = GetNodeOrNull<Control>("background/CenterPanel/ShipPreview/Selector") ?? GetNodeOrNull<Control>("CenterPanel/ShipPreview/Selector");
        selectorRect = GetNodeOrNull<ColorRect>("background/CenterPanel/ShipPreview/Selector/SelectorRect") ?? GetNodeOrNull<ColorRect>("CenterPanel/ShipPreview/Selector/SelectorRect");
        tooltip = GetNodeOrNull<Label>("background/Tooltip") ?? GetNodeOrNull<Label>("Tooltip");

        // fallback: try finding by node name under this PauseMenu (handles nodes reordered or parent changes)
        if (trajList == null) trajList = FindChild("TrajectoryList", true, false) as ItemList;
        if (dmgList == null) dmgList = FindChild("DamageList", true, false) as ItemList;
        if (shipPreview == null) shipPreview = FindChild("ShipPreview", true, false) as Control;
        if (shipSprite == null) shipSprite = FindChild("ShipSprite", true, false) as Sprite2D;
        if (selector == null) selector = FindChild("Selector", true, false) as Control;
        if (selectorRect == null) selectorRect = FindChild("SelectorRect", true, false) as ColorRect;
        if (tooltip == null) tooltip = FindChild("Tooltip", true, false) as Label;

        // final fallback: search the whole scene tree (non-ideal but keeps UI alive)
        if (trajList == null) trajList = GetTree().GetRoot().FindChild("TrajectoryList", true, false) as ItemList;
        if (dmgList == null) dmgList = GetTree().GetRoot().FindChild("DamageList", true, false) as ItemList;
        if (shipPreview == null) shipPreview = GetTree().GetRoot().FindChild("ShipPreview", true, false) as Control;
        if (shipSprite == null) shipSprite = GetTree().GetRoot().FindChild("ShipSprite", true, false) as Sprite2D;
        if (selector == null) selector = GetTree().GetRoot().FindChild("Selector", true, false) as Control;
        if (selectorRect == null) selectorRect = GetTree().GetRoot().FindChild("SelectorRect", true, false) as ColorRect;
        if (tooltip == null) tooltip = GetTree().GetRoot().FindChild("Tooltip", true, false) as Label;

        // find player then functions manager
        var playerNode = GetTree().GetFirstNodeInGroup("player") as Node;
        if (playerNode == null)
            playerNode = GetTree().Root.FindChild("player", true, false) as Node;

        if (playerNode != null)
        {
            functionsManager = playerNode.GetNodeOrNull<FunctionsManager>("Scripts/FunctionsManager");
        }

        if (functionsManager == null)
        {
            // last resort: search whole tree for FunctionsManager node
            functionsManager = GetTree().GetRoot().FindChild("FunctionsManager", true, false) as FunctionsManager;
        }

        PopulateLists();

        if (trajList != null)
            trajList.ItemSelected += OnTrajSelected;
        if (dmgList != null)
            dmgList.ItemSelected += OnDmgSelected;

        if (selector != null)
            selector.MouseFilter = MouseFilterEnum.Stop;
        // ensure we receive input and processing
        SetProcess(true);
        SetProcessInput(true);

        // connect GUI input on selector for reliable dragging
        if (selector != null)
            selector.Connect("gui_input", new Callable(this, nameof(OnSelectorGuiInput)));

        // apply preview scale to ship and rendering
        if (shipSprite != null)
        {
            originalShipScale = shipSprite.Scale;
            shipSprite.Scale = originalShipScale * previewScale;
        }
        renderScale = previewScale;
        // defer sizing until after layout to avoid RectSize being zero in some runtime setups
        CallDeferred(nameof(DeferredApplyMinSize));
        CallDeferred(nameof(DeferredPositionShipAndSelector));

        // diagnostic: report what nodes were found at startup
        GD.Print($"PauseMenu: trajList={(trajList!=null)} dmgList={(dmgList!=null)} shipPreview={(shipPreview!=null)} shipSprite={(shipSprite!=null)} selector={(selector!=null)} selectorRect={(selectorRect!=null)} tooltip={(tooltip!=null)} functionsManager={(functionsManager!=null)}");
    }

    private void DeferredApplyMinSize()
    {
        if (trajList != null && trajList.Size == Vector2.Zero)
            trajList.CustomMinimumSize = new Vector2(220, 200);
        if (dmgList != null && dmgList.Size == Vector2.Zero)
            dmgList.CustomMinimumSize = new Vector2(220, 200);
    }

    private void DeferredPositionShipAndSelector()
    {
        if (shipPreview == null || shipSprite == null) return;

        // center ship inside preview (use global positions so drawing coordinates match)
        var previewGlobal = shipPreview.GlobalPosition;
        var previewSize = shipPreview.Size;
        shipSprite.GlobalPosition = previewGlobal + previewSize / 2f;

        // place selector to the right of ship center
        if (selector != null)
            selector.GlobalPosition = shipSprite.GlobalPosition + new Vector2(60f * previewScale, 0f);

        UpdateSelectorVisuals();
    }

    private void PopulateLists()
    {
        if (trajList != null) trajList.Clear();
        if (dmgList != null) dmgList.Clear();

        if (functionsManager == null)
        {
            if (trajList != null) trajList.AddItem("Functions manager not found");
            if (dmgList != null) dmgList.AddItem("Functions manager not found");
            return;
        }

        var trajs = functionsManager.GetUnlockedTrajectoryFunctions();
        var dmgs = functionsManager.GetUnlockedDamageFunctions();

        GD.Print($"PauseMenu.PopulateLists: trajs={(trajs==null?-1:trajs.Count)} dmgs={(dmgs==null?-1:dmgs.Count)}");

        if (trajs != null)
        {
            for (int i = 0; i < trajs.Count; i++)
            {
                var desc = trajs[i].Description ?? $"Trajectory {i+1}";
                if (trajList != null) trajList.AddItem(desc);
            }
                if (trajs.Count > 0 && selectedTraj < 0)
                {
                    selectedTraj = 0;
                    if (trajList != null) trajList.Select(0);
                }
        }
        else
        {
            if (trajList != null) trajList.AddItem("No trajectory functions");
        }

        if (dmgs != null)
        {
            for (int i = 0; i < dmgs.Count; i++)
            {
                var desc = dmgs[i].Description ?? $"Damage {i+1}";
                if (dmgList != null) dmgList.AddItem(desc);
            }
                if (dmgs.Count > 0 && selectedDmg < 0)
                {
                    selectedDmg = 0;
                    if (dmgList != null) dmgList.Select(0);
                }
        }
        else
        {
                if (dmgList != null) dmgList.AddItem("No damage functions");
        }

        UpdateSelectorVisuals();
        // apply selections to attack immediately
        ApplySelectionsToAttack();
    }

    // reflection helper removed; using public getters on FunctionsManager now

    private void OnTrajSelected(long idx)
    {
        GD.Print(idx);
        selectedTraj = (int)idx;
        UpdateSelectorVisuals();
        ApplySelectionsToAttack();
    }

    private void OnDmgSelected(long idx)
    {
        selectedDmg = (int)idx;
        UpdateSelectorVisuals();
        ApplySelectionsToAttack();
    }

    public override void _Input(InputEvent @event)
    {
        if (selector == null || shipPreview == null) return;

        if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left)
        {
            if (mb.Pressed)
            {
                // start dragging if click is near selector
                if (selector != null && shipPreview != null && GetGlobalMousePosition().DistanceTo(selector.GlobalPosition) <= 24f)
                {
                    dragging = true;
                    if (shipPreview != null) selectorOffset = selector.Position - shipPreview.GetLocalMousePosition();
                }
            }
            else
            {
                dragging = false;
            }
        }
        else if (@event is InputEventMouseMotion mm && dragging)
        {
            if (shipPreview != null)
            {
                var local = shipPreview.GetLocalMousePosition() + selectorOffset;
                if (selector != null) selector.Position = local;
            }
            UpdateSelectorVisuals();
        }
    }

    private void OnSelectorGuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left)
        {
                if (mb.Pressed)
                {
                    dragging = true;
                    if (selector != null && shipPreview != null) selectorOffset = selector.Position - shipPreview.GetLocalMousePosition();
                }
            else
            {
                dragging = false;
            }
        }
    }

    private void UpdateSelectorVisuals()
    {
        // update selector color depending on selection
        if (selectorRect != null)
        {
            if (selectedTraj >= 0 && selectedDmg >= 0)
                selectorRect.Color = new Color(1, 1, 1, 0.8f);
            else if (selectedTraj >= 0)
                selectorRect.Color = new Color(1, 1, 1, 0.6f);
            else if (selectedDmg >= 0)
                selectorRect.Color = new Color(1, 1, 1, 0.6f);
            else
                selectorRect.Color = new Color(1, 1, 1, 0.4f);
        }

        QueueRedraw();
    }

    public override void _Draw()
    {
        base._Draw();

        if (shipSprite == null || selector == null) return;

        // draw functions lines from center (use ship sprite position)
        if (shipSprite == null) return;

        Vector2 centerGlobal = shipSprite.GlobalPosition;
        Vector2 centerLocal = centerGlobal - this.GlobalPosition;

        if (functionsManager == null) return;

        var trajs = functionsManager.GetUnlockedTrajectoryFunctions();
        var dmgs = functionsManager.GetUnlockedDamageFunctions();

        // draw selected functions with higher alpha
        // prepare drawn samples and render both functions together so we can normalize damage
        Func<float, float> trajFn = null;
        Func<float, float> dmgFn = null;

        if (selectedTraj >= 0 && trajs != null && selectedTraj < trajs.Count)
            trajFn = (x) => (float)trajs[selectedTraj].FunctionDefinition.DynamicInvoke(x);

        if (selectedDmg >= 0 && dmgs != null && selectedDmg < dmgs.Count)
            dmgFn = (x) => (float)dmgs[selectedDmg].FunctionDefinition.DynamicInvoke(x);

        if (trajFn != null || dmgFn != null)
            DrawFunctionsPreview(trajFn, dmgFn, centerLocal);
    }
    private void DrawFunctionsPreview(Func<float, float> trajFn, Func<float, float> dmgFn, Vector2 centerLocal)
    {
        int samples = 120;
        float step = 10f; // pixels per sample before scaling

        var trajPoints = new Vector2[samples];
        var dmgPoints = new Vector2[samples];

        float trajMin = float.MaxValue, trajMax = float.MinValue;
        float dmgMin = float.MaxValue, dmgMax = float.MinValue;

        for (int i = 0; i < samples; i++)
        {
            float d = i * step;
            float x = d * renderScale;
            if (trajFn != null)
            {
                float yv = trajFn(d * 0.01f);
                trajMin = Mathf.Min(trajMin, yv);
                trajMax = Mathf.Max(trajMax, yv);
                trajPoints[i] = new Vector2(x, -yv * 100f * renderScale);
            }
            if (dmgFn != null)
            {
                float yv = dmgFn(d * 0.01f);
                dmgMin = Mathf.Min(dmgMin, yv);
                dmgMax = Mathf.Max(dmgMax, yv);
                dmgPoints[i] = new Vector2(x, -yv * 100f * renderScale);
                // store raw damage value before any normalization/mapping
                // ensure array sized
                // (we'll copy into lastRawDmgValues later)
            }
        }

        // normalize damage to trajectory min/max if both present
        if (trajFn != null && dmgFn != null)
        {
            float trajRange = Mathf.Max(1e-6f, trajMax - trajMin);
            float dmgRange = Mathf.Max(1e-6f, dmgMax - dmgMin);
            for (int i = 0; i < samples; i++)
            {
                float dmgY = dmgPoints[i].Y;
                // invert back to function-space value
                float dmgVal = -dmgY / (100f * renderScale);
                // normalize into 0..1 relative to dmgMin..dmgMax
                float norm = (dmgVal - dmgMin) / dmgRange;
                // map into trajMin..trajMax
                float mapped = trajMin + norm * trajRange;
                dmgPoints[i] = new Vector2(dmgPoints[i].X, -mapped * 100f * renderScale);
            }
        }

        // rotate by selector angle
        if (selector == null || shipSprite == null) return; 
        float angle = (selector.GlobalPosition - shipSprite.GlobalPosition).Angle();
        var rot = new Transform2D(angle, Vector2.Zero);

        // prepare cached global points and values for hover detection
        lastTrajGlobalPoints = new Vector2[samples];
        lastDmgGlobalPoints = new Vector2[samples];
        lastSampleDistances = new float[samples];
        lastTrajValues = new float[samples];
        lastDmgValues = new float[samples];
        lastRawDmgValues = new float[samples];

        for (int i = 1; i < samples; i++)
        {
            if (trajFn != null)
            {
                Vector2 p1 = rot.BasisXform(trajPoints[i - 1]) + centerLocal;
                Vector2 p2 = rot.BasisXform(trajPoints[i]) + centerLocal;
                DrawLine(p1, p2, new Color(1,1,1, selectedTraj >= 0 ? 0.9f : 0.4f), 2);
                lastTrajGlobalPoints[i] = p2;
                lastTrajValues[i] = -trajPoints[i].Y / (100f * renderScale);
                lastSampleDistances[i] = (rot.BasisXform(trajPoints[i]).X);
            }
            if (dmgFn != null)
            {
                Vector2 p1 = rot.BasisXform(dmgPoints[i - 1]) + centerLocal;
                Vector2 p2 = rot.BasisXform(dmgPoints[i]) + centerLocal;
                DrawLine(p1, p2, new Color(1,0,0, selectedDmg >= 0 ? 0.9f : 0.4f), 2);
                lastDmgGlobalPoints[i] = p2;
                lastDmgValues[i] = -dmgPoints[i].Y / (100f * renderScale);
                // raw damage before normalization
                lastRawDmgValues[i] = (dmgFn != null) ? dmgFn(i * step * 0.01f) : 0f;
            }
        }
    }

    public override void _Process(double delta)
    {
        // if we don't have functionsManager yet, try to find it (player may be created after PauseMenu)
        if (functionsManager == null)
        {
            var playerNode = GetTree().GetFirstNodeInGroup("player") as Node;
            if (playerNode == null)
                playerNode = GetTree().Root.FindChild("player", true, false) as Node;
            if (playerNode != null)
            {
                functionsManager = playerNode.GetNodeOrNull<FunctionsManager>("Scripts/FunctionsManager");
                if (functionsManager != null)
                    PopulateLists();
            }
        }

        // update dragging during process (reliable even if _Input isn't firing)
        if (dragging && shipPreview != null && selector != null)
        {
            var local = shipPreview.GetLocalMousePosition() + selectorOffset;
            if (selector != null) selector.Position = local;
            UpdateSelectorVisuals();
            // show tooltip at selector while dragging
            ShowTooltipForSelection();
        }

        // handle hover tooltip
        Vector2 mouse = GetGlobalMousePosition();
        // hover: check nearest sampled point on either function
        float bestDist = 9999f;
        int bestIdx = -1;
        bool bestIsTraj = false;
        if (lastTrajGlobalPoints != null)
        {
            for (int i = 0; i < lastTrajGlobalPoints.Length; i++)
            {
                var p = lastTrajGlobalPoints[i];
                if (p == Vector2.Zero) continue;
                float d = mouse.DistanceTo(p);
                if (d < bestDist)
                {
                    bestDist = d; bestIdx = i; bestIsTraj = true;
                }
            }
        }
        if (lastDmgGlobalPoints != null)
        {
            for (int i = 0; i < lastDmgGlobalPoints.Length; i++)
            {
                var p = lastDmgGlobalPoints[i];
                if (p == Vector2.Zero) continue;
                float d = mouse.DistanceTo(p);
                if (d < bestDist)
                {
                    bestDist = d; bestIdx = i; bestIsTraj = false;
                }
            }
        }

        // If mouse is near the selector, show selector tooltip (priority)
        if (selector != null && mouse.DistanceTo(selector.GlobalPosition) <= 24f)
        {
            ShowTooltipAt(mouse);
        }
        else if (bestIdx >= 0 && bestDist <= 16f)
        {
            // use actual mouse position (continuous) to compute values
            ShowTooltipAt(mouse);
        }
        else
        {
            if (tooltip != null) { tooltip.Text = ""; tooltip.Visible = false; }
        }
    }

    // Evaluate selected functions at a global point (continuous distance) and show tooltip
    private void ShowTooltipAt(Vector2 globalPoint)
    {
        if (functionsManager == null || tooltip == null || shipSprite == null) return;

        var trajs = functionsManager.GetUnlockedTrajectoryFunctions();
        var dmgs = functionsManager.GetUnlockedDamageFunctions();

        Vector2 center = shipSprite.GlobalPosition;
        Vector2 dir = GetFireDirection();
        // project the point onto firing axis to get signed x in pixels
        float x = (globalPoint - center).Dot(dir);

        float y = 0f;
        float dmg = 0f;

        if (trajs != null && selectedTraj >= 0 && selectedTraj < trajs.Count)
        {
            var fn = trajs[selectedTraj].FunctionDefinition as Delegate;
            try { y = (float)fn.DynamicInvoke(x * 0.01f); } catch { y = 0f; }
        }

        if (dmgs != null && selectedDmg >= 0 && selectedDmg < dmgs.Count)
        {
            var fn = dmgs[selectedDmg].FunctionDefinition as Delegate;
            try { dmg = (float)fn.DynamicInvoke(x * 0.01f); } catch { dmg = 0f; }
        }

        string trajDesc = (trajs != null && selectedTraj >= 0 && selectedTraj < trajs.Count) ? trajs[selectedTraj].Description : "-";
        string dmgDesc = (dmgs != null && selectedDmg >= 0 && selectedDmg < dmgs.Count) ? dmgs[selectedDmg].Description : "-";

        tooltip.Text = $"x={x:F2} y={y:F2} dmg={dmg:F2}\ntraj: {trajDesc}\ndmg: {dmgDesc}";
        tooltip.Visible = true;
        var tipSize = tooltip.Size;
        tooltip.GlobalPosition = globalPoint + new Vector2(12f, -tipSize.Y - 8f);
    }

    // Helper: get firing direction (unit vector) from ship center toward selector
    private Vector2 GetFireDirection()
    {
        if (shipSprite == null) return Vector2.Right;
        if (selector != null)
        {
            Vector2 v = selector.GlobalPosition - shipSprite.GlobalPosition;
            if (v.Length() > 0.0001f) return v.Normalized();
        }
        return Vector2.Right;
    }

    private void ShowTooltipForSelection()
    {
        if (functionsManager == null || tooltip == null || shipSprite == null || selector == null) return;

        var trajs = functionsManager.GetUnlockedTrajectoryFunctions();
        var dmgs = functionsManager.GetUnlockedDamageFunctions();

        if (selector == null || shipSprite == null) return;
        // use mouse position projected onto firing axis so tooltip follows the mouse inside the selector square
        Vector2 mouse = GetGlobalMousePosition();
        Vector2 center = shipSprite.GlobalPosition;
        Vector2 dir = GetFireDirection();
        float x = (mouse - center).Dot(dir);
        float y = 0f;
        float dmg = 0f;

        if (trajs != null && selectedTraj >= 0 && selectedTraj < trajs.Count)
        {
            var fn = trajs[selectedTraj].FunctionDefinition as Delegate;
            try { y = (float)fn.DynamicInvoke(x * 0.01f); } catch { y = 0f; }
        }

        if (dmgs != null && selectedDmg >= 0 && selectedDmg < dmgs.Count)
        {
            var fn = dmgs[selectedDmg].FunctionDefinition as Delegate;
            try { dmg = (float)fn.DynamicInvoke(x * 0.01f); } catch { dmg = 0f; }
        }

        if (tooltip != null)
        {
            string trajDesc = (trajs != null && selectedTraj >= 0 && selectedTraj < trajs.Count) ? trajs[selectedTraj].Description : "-";
            string dmgDesc = (dmgs != null && selectedDmg >= 0 && selectedDmg < dmgs.Count) ? dmgs[selectedDmg].Description : "-";
            tooltip.Text = $"x={x:F2} y={y:F2} dmg={dmg:F2}\ntraj: {trajDesc}\ndmg: {dmgDesc}";
            tooltip.Visible = true;
            // position tooltip to the right and slightly above selector, using Size
            var tipSize = tooltip.Size;
            var target = selector.GlobalPosition + new Vector2(16f, -tipSize.Y - 8f);
            tooltip.GlobalPosition = target;
        }
    }

    private void ShowTooltipForSample(int idx, bool isTraj)
    {
        if (lastSampleDistances == null || tooltip == null || shipSprite == null) return;
        // use mouse projection along firing axis to compute continuous x
        Vector2 center = shipSprite.GlobalPosition;
        Vector2 dir = GetFireDirection();
        Vector2 mouse = GetGlobalMousePosition();
        float x = (mouse - center).Dot(dir);

        float y = 0f; float dmg = 0f;
        var trajs = functionsManager?.GetUnlockedTrajectoryFunctions();
        var dmgs = functionsManager?.GetUnlockedDamageFunctions();
        if (trajs != null && selectedTraj >= 0 && selectedTraj < trajs.Count)
        {
            var fn = trajs[selectedTraj].FunctionDefinition as Delegate;
            try { y = (float)fn.DynamicInvoke(x * 0.01f); } catch { y = 0f; }
        }
        if (dmgs != null && selectedDmg >= 0 && selectedDmg < dmgs.Count)
        {
            var fn = dmgs[selectedDmg].FunctionDefinition as Delegate;
            try { dmg = (float)fn.DynamicInvoke(x * 0.01f); } catch { dmg = 0f; }
        }

        if (tooltip != null)
        {
            tooltip.Text = $"x={x:F2} y={y:F2} dmg={dmg:F2}";
            tooltip.Visible = true;
            var tipSize = tooltip.Size;
            tooltip.GlobalPosition = mouse + new Vector2(12f, -tipSize.Y - 8f);
        }
    }

    private void ApplySelectionsToAttack()
    {
        if (functionsManager == null) return;
        var trajs = functionsManager.GetUnlockedTrajectoryFunctions();
        var dmgs = functionsManager.GetUnlockedDamageFunctions();
        if (trajs == null || dmgs == null) return;
        if (selectedTraj >= 0 && selectedTraj < trajs.Count && selectedDmg >= 0 && selectedDmg < dmgs.Count)
        {
            functionsManager.SetAttackFunctions(0, trajs[selectedTraj], dmgs[selectedDmg]);
        }
    }
}
