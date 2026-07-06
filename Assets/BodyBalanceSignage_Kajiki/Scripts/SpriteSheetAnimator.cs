using UnityEngine;
using BodyBalanceSignage;

public class SpriteSheetAnimator : MonoBehaviour
{
    public enum MoveState
    {
        Forward,
        Backward
    }

    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private CameraMover cameraMover;
    [SerializeField] private float fps = 5f;

    private Material material;

    private float timer;
    private int currentFrame;

    private float frameSizeX = 0.25f;

    private MoveState currentState = MoveState.Forward;

    private int startFrame;
    private int endFrame;

    private float lastZ;

    void Start()
    {
        material = targetRenderer.material;
        material.mainTextureScale = new Vector2(frameSizeX, 1f);

        SetState(MoveState.Forward, true);

        if (cameraMover != null)
        {
            lastZ = cameraMover.transform.localPosition.z;
        }
    }

    void Update()
    {
        HandleDirectionCheck(); // ★ここで直接監視

        timer += Time.deltaTime;

        if (timer >= 1f / fps)
        {
            timer = 0f;

            currentFrame++;

            if (currentFrame > endFrame)
                currentFrame = startFrame;

            ApplyFrame(currentFrame);
        }
    }

    void HandleDirectionCheck()
    {
        if (cameraMover == null) return;

        float z = cameraMover.transform.localPosition.z;

        float delta = z - lastZ;

        if (Mathf.Abs(delta) > 0.001f)
        {
            bool isForward = delta > 0f;

            SetState(isForward ? MoveState.Forward : MoveState.Backward);
        }

        lastZ = z;
    }

    public void SetState(MoveState state, bool force = false)
    {
        if (!force && currentState == state) return;

        currentState = state;

        if (state == MoveState.Forward)
        {
            startFrame = 0;
            endFrame = 1;
        }
        else
        {
            startFrame = 2;
            endFrame = 3;
        }

        currentFrame = Mathf.Clamp(currentFrame, startFrame, endFrame);

        ApplyFrame(currentFrame);
    }

    void ApplyFrame(int frame)
    {
        material.mainTextureOffset = new Vector2(frame * frameSizeX, 0f);
    }
}