using Celeste.Mod.Entities;
using FMOD.Studio;
using LuckyHelper.Extensions;
using LuckyHelper.Modules;
using Timer = LuckyHelper.Utils.Timer;

namespace LuckyHelper.Entities.Texts;

[CustomEntity(
    "LuckyHelper/FilmTextRaw",
    "LuckyHelper/FilmTextBSide",
    "LuckyHelper/FilmTextTypeInOut",
    "LuckyHelper/FilmTextTypeInFadeOut"
)]
[Tracked]
public class FilmText : Entity
{
    public enum PositionMode
    {
        World,
        Screen
    }

    public enum TriggerMode
    {
        OnRoomLoaded,
        OnFlag
    }

    private PositionMode positionMode;
    private TriggerMode triggerMode;
    private string triggerFlag;
    private string dialogID;

    public enum ShowMode
    {
        FadeIn,
        FadeOut,
        SlideIn,
        SlideOut,
        TypeIn,
        TypeOut,
    }

    // 左滑入, 右划出, B-side
    // 左滑入左划出, 草莓 hud

    private float textScale = 1;
    private Color textColor = Color.White;
    private bool showOutline;
    private float outlineThickness;
    private Color textOutlineColor = Color.Black;

    private ShowController fadeController;
    private ShowController typeController;
    private ShowController slideController;

    private float textAlpha;
    private int showCharacterCount;
    private Vector2 textOffset;

    private bool started;
    private bool playTypeAudio;
    private float simulationSpeed;
    private Vector2 globalOffset;

    private string Content => Dialog.Clean(dialogID);

    public FilmText(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Tag |= Tags.HUD;

        positionMode = data.Enum<PositionMode>("positionMode");
        triggerMode = data.Enum<TriggerMode>("triggerMode");
        dialogID = data.Attr("dialogID");
        triggerFlag = data.Attr("triggerFlag");

        textScale = data.Float("textScale");
        textColor = data.HexColor("textColor");
        showOutline = data.Bool("showOutline");
        outlineThickness = data.Float("outlineThickness");
        textOutlineColor = data.HexColor("textOutlineColor");

        playTypeAudio = data.Bool("playTypeAudio");
        simulationSpeed = data.Float("simulationSpeed");

        fadeController = new ShowController()
        {
            parent = this,
            inTimer = new Timer(data.Float("fadeInTime")),
            inEase = EaseModule.EaseTypes[data.Attr("fadeInEase", "Linear")],
            outTimer = new Timer(data.Float("fadeOutTime")),
            outEase = EaseModule.EaseTypes[data.Attr("fadeOutEase", "Linear")],
            outDelayTimer = new Timer(data.Float("fadeOutDelayTime")),
            inSetter = (percent) => { textAlpha = percent; },
            outSetter = (percent) => { textAlpha = 1 - percent; },
        };

        typeController = new TypeController()
        {
            parent = this,
            inTimer = new Timer(data.Float("typeInTime")),
            inEase = EaseModule.EaseTypes[data.Attr("typeInEase", "Linear")],
            outTimer = new Timer(data.Float("typeOutTime")),
            outEase = EaseModule.EaseTypes[data.Attr("typeOutEase", "Linear")],
            outDelayTimer = new Timer(data.Float("typeOutDelayTime")),
            inSetter = (percent) => { showCharacterCount = (int)Math.Max(0, Math.Ceiling(percent * (Content.Length + 1)) - 1); },
            outSetter = (percent) => { showCharacterCount = (int)Math.Max(0, Math.Ceiling((1 - percent) * (Content.Length + 1)) - 1); },
            sfx = Audio.CreateInstance("event:/LuckyHelper/FilmText/Type")
        };

        slideController = new SlideController()
        {
            parent = this,
            inTimer = new Timer(data.Float("slideInTime")),
            inEase = EaseModule.EaseTypes[data.Attr("slideInEase", "Linear")],
            slideInOffset = new Vector2(data.Float("slideInOffsetX"), data.Float("slideInOffsetY")),
            outTimer = new Timer(data.Float("slideOutTime")),
            outEase = EaseModule.EaseTypes[data.Attr("slideOutEase", "Linear")],
            slideOutOffset = new Vector2(data.Float("slideOutOffsetX"), data.Float("slideOutOffsetY")),
            globalSlideSpeed = new Vector2(data.Float("globalSlideSpeedX"), data.Float("globalSlideSpeedY")),
            outDelayTimer = new Timer(data.Float("slideOutDelayTime")),
        };


        textAlpha = 1;
        showCharacterCount = Content.Length;

        fadeController.Init();
        typeController.Init();
        slideController.Init();
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);

        if (triggerMode == TriggerMode.OnRoomLoaded)
            Triggered();
    }

    private void Triggered()
    {
        started = true;
        fadeController.Start();
        typeController.Start();
        slideController.Start();
    }

    public override void Update()
    {
        base.Update();

        if (triggerMode == TriggerMode.OnFlag && this.Session().GetFlag(triggerFlag))
        {
            this.Session().SetFlag(triggerFlag, false);
            Triggered();
        }

        if (!started)
            return;

        fadeController.Update();
        typeController.Update();
        slideController.Update();
    }

    public override void Render()
    {
        base.Render();

        Vector2 textPosition = GetTextPosition() + textOffset + globalOffset;
        DrawText(Content.Substring(0, showCharacterCount), textPosition);
    }

    private void DrawText(string content, Vector2 textPosition)
    {
        if (showOutline)
            ActiveFont.DrawOutline(content, textPosition, new Vector2(0f, 0f), Vector2.One * textScale, textColor * textAlpha, outlineThickness, textOutlineColor * textAlpha);
        else
            ActiveFont.Draw(content, textPosition, new Vector2(0f, 0f), Vector2.One * textScale, textColor * textAlpha);
    }

    private Vector2 GetTextPosition()
    {
        Vector2 pivotPosition = this.Level().Bounds.Position();
        if (positionMode == PositionMode.World)
        {
            Camera camera = SceneAs<Level>().Camera;
            pivotPosition = camera.Position;
        }
        else
        {
            FilmTextPivot pivot = this.Tracker().GetEntity<FilmTextPivot>();
            if (pivot != null)
                pivotPosition = pivot.Position;
        }

        Vector2 relativePosition = (Position - pivotPosition) * 6;
        return relativePosition;
    }


    public class ShowController
    {
        public FilmText parent;
        public Timer inTimer;
        public Timer outDelayTimer;
        public Timer outTimer;

        public Ease.Easer inEase;
        public Ease.Easer outEase;

        public Action<float> inSetter;
        public Action<float> outSetter;

        // -1: 未启动, 0: showIn, 1: waiting(delaying), 2: showOut
        private int state = -1;

        public virtual void Init()
        {
            if (inTimer.Duration > 0)
            {
                inSetter(0);
            }
            else
            {
                inSetter(1);
            }
        }

        public void Start()
        {
            if (inTimer.Duration > 0)
            {
                inTimer.Restart();
                state = 0;
            }

            if (outDelayTimer.Duration > 0)
            {
                outDelayTimer.Restart();
                if (state == -1)
                    state = 1;
            }

            if (outTimer.Duration > 0)
            {
                outTimer.Restart();
                if (state == -1)
                    state = 2;
            }
        }

        public virtual void Update()
        {
            float deltaTime = Engine.DeltaTime * parent.simulationSpeed;
            if (state == 0)
            {
                inTimer.Update(deltaTime);
                inSetter(inEase(inTimer.Percent));
                if (inTimer.Over)
                    MoveToNextState();
            }
            else if (state == 1)
            {
                outDelayTimer.Update(deltaTime);
                if (outDelayTimer.Over)
                    MoveToNextState();
            }
            else if (state == 2)
            {
                outTimer.Update(deltaTime);
                outSetter(outEase(outTimer.Percent));
                if (outTimer.Over)
                    MoveToNextState();
            }
        }

        private void MoveToNextState()
        {
            if (state == -1)
                return;
            if (state == 0)
            {
                if (outDelayTimer.Over)
                {
                    if (outTimer.Over)
                        state = -1;
                    else
                        state = 2;
                }
                else
                    state = 1;
            }
            else if (state == 1)
            {
                if (outTimer.Over)
                    state = -1;
                else
                    state = 2;
            }
            else
                state = -1;
        }
    }

    public class TypeController : ShowController
    {
        public EventInstance sfx;
        private bool started;
        private int preShowCharacterCount;

        public override void Init()
        {
            base.Init();
            preShowCharacterCount = parent.showCharacterCount;
        }

        public override void Update()
        {
            base.Update();
            if (!parent.playTypeAudio)
                return;

            if (preShowCharacterCount != parent.showCharacterCount)
            {
                if (!started)
                {
                    started = true;
                    sfx.start();
                }

                sfx.setParameterValue("variant", Calc.Random.Range(0, 7));
                preShowCharacterCount = parent.showCharacterCount;
            }
        }
    }

    public class SlideController : ShowController
    {
        public Vector2 slideInOffset;
        public Vector2 slideOutOffset;
        public Vector2 globalSlideSpeed;

        public override void Init()
        {
            inSetter = percent => { parent.textOffset = Vector2.Lerp(slideInOffset, Vector2.Zero, percent); };
            outSetter = percent => { parent.textOffset = Vector2.Lerp(Vector2.Zero, slideOutOffset, percent); };
            base.Init();
        }

        public override void Update()
        {
            base.Update();
            float deltaTime = Engine.DeltaTime * parent.simulationSpeed;
            parent.globalOffset += deltaTime * globalSlideSpeed;
        }
    }
}