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

    // multi-selector support
    private List<Control> selectors = new();
    private List<ColorRect> selectorRects = new();
    private int activeSelector = 0; // which attack slot is being edited

    private FunctionsManager functionsManager;

    private int[] selectedTraj = null;
    private int[] selectedDmg = null;

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

        // initialize per-attack selections after we know attack count
        int atkCount = functionsManager != null ? functionsManager.GetAttackCount() : 0;
        if (atkCount <= 0) atkCount = 1; // fallback to single selector
        selectedTraj = new int[atkCount];
        selectedDmg = new int[atkCount];
        for (int i = 0; i < atkCount; i++) { selectedTraj[i] = -1; selectedDmg[i] = -1; }

        CreateSelectors(atkCount);

        PopulateLists();

        if (trajList != null)
            trajList.Connect("item_selected", new Callable(this, nameof(OnTrajSelected)));
        if (dmgList != null)
            dmgList.Connect("item_selected", new Callable(this, nameof(OnDmgSelected)));

        // set mouse filter on all selectors
        foreach (var sel in selectors)
            sel.MouseFilter = MouseFilterEnum.Stop;
        // ensure we receive input and processing
        SetProcess(true);
        SetProcessInput(true);

        // NOTE: we intentionally do not connect selector GUI input to avoid duplicate
        // drag handling between `_Input` and `gui_input` which caused snapping.

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

    private void CreateSelectors(int count)
    {
        // free old selector nodes (if any)
        foreach (var old in selectors)
        {
            if (old != null && old.IsInsideTree())
                old.QueueFree();
        }
        selectors.Clear();
        selectorRects.Clear();

        Node parent = shipPreview != null ? (Node)shipPreview : GetTree().GetRoot();

        for (int i = 0; i < count; i++)
        {
            var s = new Control();
            s.Name = $"Selector_{i}";
            s.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.CenterTop);
            s.Size = new Vector2(16, 16);
            parent.AddChild(s);

            var r = new ColorRect();
            r.Name = "SelectorRect";
            r.Color = new Color(1, 1, 1, 0.6f);
            r.Size = new Vector2(16, 16);
            r.AnchorLeft = 0.5f; r.AnchorTop = 0.5f; r.AnchorRight = 0.5f; r.AnchorBottom = 0.5f;
            r.Position = new Vector2(-16, -16);
            s.AddChild(r);

            var lbl = new Label();
            lbl.Name = "IndexLabel";
            lbl.Text = (i + 1).ToString();
            lbl.AddThemeColorOverride("font_color", new Color(0,0,0,1));
            lbl.AnchorLeft = 0.5f; lbl.AnchorTop = 0.5f; lbl.AnchorRight = 0.5f; lbl.AnchorBottom = 0.5f;
            lbl.Position = new Vector2(-12, -20);
            s.AddChild(lbl);

            selectors.Add(s);
            selectorRects.Add(r);
            GD.Print($"PauseMenu: created selector[{i}] name={s.Name} parent={(parent!=null?parent.Name:"null")} rect={(r!=null)}");
        }

        // set initial active selector
        activeSelector = 0;

        // hide original template selector so it doesn't overlap
        if (selector != null && selector.IsInsideTree())
        {
            selector.Visible = false;
        }
        if (selectorRect != null && selectorRect.IsInsideTree())
        {
            selectorRect.Visible = false;
        }
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
        if (dragging) return;
        if (shipPreview == null || shipSprite == null) return;

        // center ship inside preview (use global positions so drawing coordinates match)
        var previewGlobal = shipPreview.GlobalPosition;
        var previewSize = shipPreview.Size;
        shipSprite.GlobalPosition = previewGlobal + previewSize / 2f;

        // place selectors to the right of ship center in a small arc
        if (selectors != null && selectors.Count > 0)
        {
            int count = selectors.Count;
            for (int i = 0; i < count; i++)
            {
                var s = selectors[i];
                if (s == null) continue;
                float angle = 0f;
                if (functionsManager != null)
                {
                    // use stored rotation for each attack if present
                    angle = functionsManager.GetAttackRotation(i);
                }
                else
                {
                    if (count > 1)
                        angle = Mathf.Lerp(-0.25f, 0.25f, (float)i / (count - 1));
                }
                var offset = new Vector2(60f * previewScale, 0f).Rotated(angle);
                s.GlobalPosition = shipSprite.GlobalPosition + offset;
            }
        }

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
                if (trajs.Count > 0)
                {
                    for (int a = 0; a < selectedTraj.Length; a++) if (selectedTraj[a] < 0) selectedTraj[a] = 0;
                    if (trajList != null) trajList.Select(selectedTraj[activeSelector]);
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
                if (dmgs.Count > 0)
                {
                    for (int a = 0; a < selectedDmg.Length; a++) if (selectedDmg[a] < 0) selectedDmg[a] = 0;
                    if (dmgList != null) dmgList.Select(selectedDmg[activeSelector]);
                }
        }
        else
        {
                if (dmgList != null) dmgList.AddItem("No damage functions");
        }

        // initialize per-slot selections from functionsManager's current attacks
        if (functionsManager != null)
        {
            int atkCount = functionsManager.GetAttackCount();
            for (int slot = 0; slot < Math.Min(atkCount, selectedTraj.Length); slot++)
            {
                var atkTraj = functionsManager.GetAttackTrajectory(slot);
                var atkDmg = functionsManager.GetAttackDamage(slot);
                if (atkTraj != null && trajs != null)
                {
                    for (int j = 0; j < trajs.Count; j++)
                    {
                        if (trajs[j].Description == atkTraj.Description)
                        {
                            selectedTraj[slot] = j; break;
                        }
                    }
                }
                if (atkDmg != null && dmgs != null)
                {
                    for (int j = 0; j < dmgs.Count; j++)
                    {
                        if (dmgs[j].Description == atkDmg.Description)
                        {
                            selectedDmg[slot] = j; break;
                        }
                    }
                }
            }
        }

        // debug: print selector info
        GD.Print($"PauseMenu: selectors.Count={selectors.Count} activeSelector={activeSelector}");
        for (int i = 0; i < selectors.Count; i++)
        {
            var s = selectors[i];
            if (s != null)
            GD.Print($"PauseMenu: selector[{i}] globalPos={s.GlobalPosition} parent={s.GetParent()?.Name}");
        }

        UpdateSelectorVisuals();
        // apply selections to attack immediately
        ApplySelectionsToAttack();
    }

    // reflection helper removed; using public getters on FunctionsManager now

    private void OnTrajSelected(int idx)
    {
        if (selectedTraj == null) return;
        if (activeSelector < 0 || activeSelector >= selectedTraj.Length) return;
        selectedTraj[activeSelector] = idx;
        UpdateSelectorVisuals();
        ApplySelectionsToAttack(activeSelector);
    }

    private void OnDmgSelected(int idx)
    {
        if (selectedDmg == null) return;
        if (activeSelector < 0 || activeSelector >= selectedDmg.Length) return;
        selectedDmg[activeSelector] = idx;
        UpdateSelectorVisuals();
        ApplySelectionsToAttack(activeSelector);
    }

    public override void _Input(InputEvent @event)
    {
        if (selectors.Count == 0 || shipPreview == null) return;

        if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left)
        {
            if (mb.Pressed)
            {
                // start dragging if click is near any selector; set activeSelector accordingly
                for (int i = 0; i < selectors.Count; i++)
                {
                    var s = selectors[i];
                    if (s != null && GetGlobalMousePosition().DistanceTo(s.GlobalPosition) <= 12f)
                    {
                        SetActiveSelector(i);
                        dragging = true;
                        // offset between selector top-left and mouse local position
                        selectorOffset = s.Position - shipPreview.GetLocalMousePosition();
                        break;
                    }
                }
            }
            else
            {
                dragging = false;
            }
        }
    }

    private void OnSelectorGuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left)
        {
                if (mb.Pressed)
                {
                    // find which selector received the input
                    for (int i = 0; i < selectors.Count; i++)
                    {
                        if (selectors[i] == null) continue;
                        // compare mouse to selector global position
                        if (GetGlobalMousePosition().DistanceTo(selectors[i].GlobalPosition) <= 12f)
                        {
                            SetActiveSelector(i);
                            dragging = true;
                            selectorOffset = selectors[i].Position - shipPreview.GetLocalMousePosition() + new Vector2(8,8);
                            break;
                        }
                    }
                }
            else
            {
                dragging = false;
            }
        }
    }

    private void SetActiveSelector(int newIdx)
    {
        if (newIdx < 0 || newIdx >= selectors.Count) return;
        activeSelector = newIdx;
        UpdateSelectorVisuals();
        // update lists to match this selector's saved selection
        if (trajList != null && selectedTraj != null && activeSelector < selectedTraj.Length)
        {
            int val = selectedTraj[activeSelector];
            if (val >= 0) trajList.Select(val); else trajList.DeselectAll();
        }
        if (dmgList != null && selectedDmg != null && activeSelector < selectedDmg.Length)
        {
            int val = selectedDmg[activeSelector];
            if (val >= 0) dmgList.Select(val); else dmgList.DeselectAll();
        }
    }

    private void UpdateSelectorVisuals()
    {
        // update selector colors: active selector green, others white
        for (int i = 0; i < selectors.Count; i++)
        {
            var rect = (i < selectorRects.Count) ? selectorRects[i] : null;
            if (rect == null) continue;
            bool hasTraj = (selectedTraj != null && selectedTraj.Length > i && selectedTraj[i] >= 0);
            bool hasDmg = (selectedDmg != null && selectedDmg.Length > i && selectedDmg[i] >= 0);
            if (i == activeSelector)
            {
                rect.Color = new Color(0, 1, 0, (hasTraj && hasDmg) ? 0.9f : 0.8f);
            }
            else
            {
                rect.Color = new Color(1, 1, 1, (hasTraj && hasDmg) ? 0.9f : 0.6f);
            }
        }

        QueueRedraw();
    }

    public override void _Draw()
    {
        base._Draw();

        if (shipSprite == null || selectors == null || selectors.Count == 0) return;

        // draw functions lines from center (use ship sprite position)

        Vector2 centerGlobal = shipSprite.GlobalPosition;
        Vector2 centerLocal = centerGlobal - this.GlobalPosition;

        if (functionsManager == null) return;

        var trajs = functionsManager.GetUnlockedTrajectoryFunctions();
        var dmgs = functionsManager.GetUnlockedDamageFunctions();

        // draw previews for all attack slots; highlight active slot
        int atkCount = functionsManager != null ? functionsManager.GetAttackCount() : 1;
        for (int slot = 0; slot < Math.Max(1, atkCount); slot++)
        {
            Func<float, float> trajFnSlot = null;
            Func<float, float> dmgFnSlot = null;
            if (selectedTraj != null && slot >= 0 && slot < selectedTraj.Length)
            {
                int t = selectedTraj[slot];
                if (t >= 0 && trajs != null && t < trajs.Count)
                    trajFnSlot = (x) => (float)trajs[t].FunctionDefinition.DynamicInvoke(x);
            }
            if (selectedDmg != null && slot >= 0 && slot < selectedDmg.Length)
            {
                int d = selectedDmg[slot];
                if (d >= 0 && dmgs != null && d < dmgs.Count)
                    dmgFnSlot = (x) => (float)dmgs[d].FunctionDefinition.DynamicInvoke(x);
            }

            var selNode = (slot >= 0 && slot < selectors.Count) ? selectors[slot] : null;
            bool isActive = (slot == activeSelector);
            if (trajFnSlot != null || dmgFnSlot != null)
                DrawFunctionsPreview(trajFnSlot, dmgFnSlot, centerLocal, selNode, slot, isActive);
        }
    }
    private void DrawFunctionsPreview(Func<float, float> trajFn, Func<float, float> dmgFn, Vector2 centerLocal, Control selNode, int slotIndex, bool isActive)
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

        // rotate by selector angle (slot-specific)
        if (selNode == null || shipSprite == null) return; 
        float angle = (selNode.GlobalPosition - shipSprite.GlobalPosition).Angle();
        var rot = new Transform2D(angle, Vector2.Zero);

        // prepare cached global points and values for hover detection only if this slot is active
        if (isActive)
        {
            lastTrajGlobalPoints = new Vector2[samples];
            lastDmgGlobalPoints = new Vector2[samples];
            lastSampleDistances = new float[samples];
            lastTrajValues = new float[samples];
            lastDmgValues = new float[samples];
            lastRawDmgValues = new float[samples];
        }

        for (int i = 1; i < samples; i++)
        {
            if (trajFn != null)
            {
                Vector2 p1 = rot.BasisXform(trajPoints[i - 1]) + centerLocal;
                Vector2 p2 = rot.BasisXform(trajPoints[i]) + centerLocal;
                // highlight based on whether this slot has a trajectory selected and whether it is active
                bool hasTraj = (selectedTraj != null && slotIndex >= 0 && slotIndex < selectedTraj.Length && selectedTraj[slotIndex] >= 0);
                float alpha = isActive ? 1.0f : (hasTraj ? 0.75f : 0.35f);
                DrawLine(p1, p2, new Color(1,1,1, alpha), 2);
                if (isActive)
                {
                    lastTrajGlobalPoints[i] = p2;
                    lastTrajValues[i] = -trajPoints[i].Y / (100f * renderScale);
                    lastSampleDistances[i] = (rot.BasisXform(trajPoints[i]).X);
                }
            }
            if (dmgFn != null)
            {
                Vector2 p1 = rot.BasisXform(dmgPoints[i - 1]) + centerLocal;
                Vector2 p2 = rot.BasisXform(dmgPoints[i]) + centerLocal;
                bool hasDmg = (selectedDmg != null && slotIndex >= 0 && slotIndex < selectedDmg.Length && selectedDmg[slotIndex] >= 0);
                float alphaD = isActive ? 1.0f : (hasDmg ? 0.75f : 0.35f);
                DrawLine(p1, p2, new Color(1,0,0, alphaD), 2);
                if (isActive)
                {
                    lastDmgGlobalPoints[i] = p2;
                    lastDmgValues[i] = -dmgPoints[i].Y / (100f * renderScale);
                    // raw damage before normalization
                    lastRawDmgValues[i] = (dmgFn != null) ? dmgFn(i * step * 0.01f) : 0f;
                }
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
                {
                    // ensure selectors and per-attack selection arrays match the attack count
                    int atkCount = functionsManager.GetAttackCount();
                    if (atkCount <= 0) atkCount = 1;
                    // only recreate if mismatch to avoid flicker
                    if (selectedTraj == null || selectedTraj.Length != atkCount)
                    {
                        selectedTraj = new int[atkCount];
                        selectedDmg = new int[atkCount];
                        for (int i = 0; i < atkCount; i++) { selectedTraj[i] = -1; selectedDmg[i] = -1; }
                        GD.Print($"PauseMenu: detected attack count={atkCount}, creating selectors");
                        CreateSelectors(atkCount);
                        GD.Print($"PauseMenu: created {selectors.Count} selectors");
                        // position and wire newly created selectors after layout
                        CallDeferred(nameof(DeferredPositionShipAndSelector));
                        foreach (var s in selectors)
                        {
                            if (s == null) continue;
                            s.MouseFilter = MouseFilterEnum.Stop;
                        }
                    }
                    PopulateLists();
                }
            }
        }

        // update dragging during process (reliable even if _Input isn't firing)
        if (dragging && shipPreview != null && activeSelector >= 0 && activeSelector < selectors.Count)
        {
            var local = shipPreview.GetLocalMousePosition() + selectorOffset;
            var s = selectors[activeSelector];
            if (s != null) s.Position = local;
            UpdateSelectorVisuals();
            // show tooltip at selector while dragging
            ShowTooltipForSelection();
            // save rotation to functions manager so attack remembers orientation
            if (functionsManager != null && shipSprite != null && s != null)
            {
                Vector2 dir = (s.GlobalPosition - shipSprite.GlobalPosition);
                if (dir.Length() > 0.0001f)
                    functionsManager.SetAttackRotation(activeSelector, dir.Angle());
            }
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
        var activeSelNode = (activeSelector >= 0 && activeSelector < selectors.Count) ? selectors[activeSelector] : null;
        if (activeSelNode != null && mouse.DistanceTo(activeSelNode.GlobalPosition) <= 24f)
        {
            ShowTooltipForSelection();
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

        if (trajs != null && selectedTraj != null && activeSelector >= 0 && activeSelector < selectedTraj.Length && selectedTraj[activeSelector] >= 0 && selectedTraj[activeSelector] < trajs.Count)
        {
            var fn = trajs[selectedTraj[activeSelector]].FunctionDefinition as Delegate;
            try { y = (float)fn.DynamicInvoke(x * 0.01f); } catch { y = 0f; }
        }

        if (dmgs != null && selectedDmg != null && activeSelector >= 0 && activeSelector < selectedDmg.Length && selectedDmg[activeSelector] >= 0 && selectedDmg[activeSelector] < dmgs.Count)
        {
            var fn = dmgs[selectedDmg[activeSelector]].FunctionDefinition as Delegate;
            try { dmg = (float)fn.DynamicInvoke(x * 0.01f); } catch { dmg = 0f; }
        }

        string trajDesc = "-";
        string dmgDesc = "-";
        if (trajs != null && selectedTraj != null && activeSelector >= 0 && activeSelector < selectedTraj.Length && selectedTraj[activeSelector] >= 0 && selectedTraj[activeSelector] < trajs.Count)
            trajDesc = trajs[selectedTraj[activeSelector]].Description;
        if (dmgs != null && selectedDmg != null && activeSelector >= 0 && activeSelector < selectedDmg.Length && selectedDmg[activeSelector] >= 0 && selectedDmg[activeSelector] < dmgs.Count)
            dmgDesc = dmgs[selectedDmg[activeSelector]].Description;

        tooltip.Text = $"x={x:F2} y={y:F2} dmg={dmg:F2}\ntraj: {trajDesc}\ndmg: {dmgDesc}";
        tooltip.Visible = true;
        var tipSize = tooltip.Size;
        tooltip.GlobalPosition = globalPoint + new Vector2(12f, -tipSize.Y - 8f);
    }

    // Helper: get firing direction (unit vector) from ship center toward selector
    private Vector2 GetFireDirection()
    {
        if (shipSprite == null) return Vector2.Right;
        var sel = (activeSelector >= 0 && activeSelector < selectors.Count) ? selectors[activeSelector] : null;
        if (sel != null)
        {
            Vector2 v = sel.GlobalPosition - shipSprite.GlobalPosition;
            if (v.Length() > 0.0001f) return v.Normalized();
        }
        return Vector2.Right;
    }

    private void ShowTooltipForSelection()
    {
        if (functionsManager == null || tooltip == null || shipSprite == null) return;

        var trajs = functionsManager.GetUnlockedTrajectoryFunctions();
        var dmgs = functionsManager.GetUnlockedDamageFunctions();

        var sel = (activeSelector >= 0 && activeSelector < selectors.Count) ? selectors[activeSelector] : null;
        if (sel == null || shipSprite == null) return;
        // use mouse position projected onto firing axis so tooltip follows the mouse inside the selector square
        Vector2 mouse = GetGlobalMousePosition();
        Vector2 center = shipSprite.GlobalPosition;
        Vector2 dir = GetFireDirection();
        float x = (mouse - center).Dot(dir);
        float y = 0f;
        float dmg = 0f;

        if (trajs != null && selectedTraj != null && activeSelector >= 0 && activeSelector < selectedTraj.Length && selectedTraj[activeSelector] >= 0 && selectedTraj[activeSelector] < trajs.Count)
        {
            var fn = trajs[selectedTraj[activeSelector]].FunctionDefinition as Delegate;
            try { y = (float)fn.DynamicInvoke(x * 0.01f); } catch { y = 0f; }
        }

        if (dmgs != null && selectedDmg != null && activeSelector >= 0 && activeSelector < selectedDmg.Length && selectedDmg[activeSelector] >= 0 && selectedDmg[activeSelector] < dmgs.Count)
        {
            var fn = dmgs[selectedDmg[activeSelector]].FunctionDefinition as Delegate;
            try { dmg = (float)fn.DynamicInvoke(x * 0.01f); } catch { dmg = 0f; }
        }

        if (tooltip != null)
        {
            string trajDesc = "-";
            string dmgDesc = "-";
            if (trajs != null && selectedTraj != null && activeSelector >= 0 && activeSelector < selectedTraj.Length && selectedTraj[activeSelector] >= 0 && selectedTraj[activeSelector] < trajs.Count)
                trajDesc = trajs[selectedTraj[activeSelector]].Description;
            if (dmgs != null && selectedDmg != null && activeSelector >= 0 && activeSelector < selectedDmg.Length && selectedDmg[activeSelector] >= 0 && selectedDmg[activeSelector] < dmgs.Count)
                dmgDesc = dmgs[selectedDmg[activeSelector]].Description;
            tooltip.Text = $"x={x:F2} y={y:F2} dmg={dmg:F2}\ntraj: {trajDesc}\ndmg: {dmgDesc}";
            tooltip.Visible = true;
            // position tooltip to the right and slightly above active selector, using Size
            var tipSize = tooltip.Size;
            var target = sel.GlobalPosition + new Vector2(16f, -tipSize.Y - 8f);
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
        if (trajs != null && selectedTraj != null && activeSelector >= 0 && activeSelector < selectedTraj.Length && selectedTraj[activeSelector] >= 0 && selectedTraj[activeSelector] < trajs.Count)
        {
            var fn = trajs[selectedTraj[activeSelector]].FunctionDefinition as Delegate;
            try { y = (float)fn.DynamicInvoke(x * 0.01f); } catch { y = 0f; }
        }
        if (dmgs != null && selectedDmg != null && activeSelector >= 0 && activeSelector < selectedDmg.Length && selectedDmg[activeSelector] >= 0 && selectedDmg[activeSelector] < dmgs.Count)
        {
            var fn = dmgs[selectedDmg[activeSelector]].FunctionDefinition as Delegate;
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
        ApplySelectionsToAttack(activeSelector);
    }

    private void ApplySelectionsToAttack(int attackIndex)
    {
        if (functionsManager == null) return;
        var trajs = functionsManager.GetUnlockedTrajectoryFunctions();
        var dmgs = functionsManager.GetUnlockedDamageFunctions();
        if (trajs == null || dmgs == null) return;
        if (selectedTraj == null || selectedDmg == null) return;
        if (attackIndex < 0 || attackIndex >= selectedTraj.Length) return;
        int t = selectedTraj[attackIndex];
        int d = selectedDmg[attackIndex];
        if (t >= 0 && t < trajs.Count && d >= 0 && d < dmgs.Count)
        {
            functionsManager.SetAttackFunctions(attackIndex, trajs[t], dmgs[d]);
        }
    }
}
