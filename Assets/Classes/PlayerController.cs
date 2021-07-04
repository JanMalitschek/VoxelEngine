using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using VoxelEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Camera")]
    public float sensitivity = 10.0f;
    public float camSmoothing = 10.0f;
    private float sRotX, sRotY;
    private float camRotX, camRotY;
    public float minCamRot, maxCamRot;

    private float uniformSpeed = 0.0f;
    [Header("Movement")]
    public float speed = 10.0f;
    public float acceleration = 5.0f;
    public float deceleration = 5.0f;
    private Vector3 inputVector;
    private Vector3 smoothInputVector;
    public float movementSmoothing = 10.0f;
    private Vector3 lastInputVector;

    [Header("Gizmos")]
    public Material gizmoMaterial;
    private Vector3Int currentSelectedVoxel = Vector3Int.zero;
    private Chunk currentSelectedChunk = null;
    private Chunk currentSelectedFaceChunk = null;
    private Vector3Int currentSelectedFaceVoxel = Vector3Int.zero;

    private Vector3Int currentChunkPos = Vector3Int.zero;
    private World world;
    private Chunk currentChunk = null;

    public enum State{
        Idling,
        Flying,
        SlowingDown
    }
    public State state = State.Idling;

    private void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        world = FindObjectOfType<VoxelEngine.World>();
    }

    private void Update() {
        //Movement
        inputVector = GetInputVector();
        smoothInputVector = Vector3.Lerp(smoothInputVector, inputVector, Time.deltaTime * movementSmoothing);

        Vector3Int chunkPos = world.WorldPositionToChunk(transform.position);
        if(currentChunkPos != chunkPos || currentChunk == null){
            currentChunk = world.GetChunk(chunkPos);
            currentChunkPos = chunkPos;
        }
        if(currentChunk != null){
            if(currentChunk.Trace(transform.position, transform.forward, 6.0f, out Chunk.ChunkTraceResult traceHit)){
                currentSelectedVoxel = traceHit.localVoxel;
                currentSelectedChunk = traceHit.chunk;
                currentSelectedFaceChunk = traceHit.hitFaceChunk;
                currentSelectedFaceVoxel = traceHit.hitFaceVoxel;
            }
            else{
                currentSelectedChunk = null;
                currentSelectedFaceChunk = null;
            }
            if(currentSelectedChunk != null && GetMiningButtonDown()){
                SoundContainer.PlayMultiGlobalSFX(VoxelContainer.GetVoxel(currentSelectedChunk.GetVoxelHashAtPosition(currentSelectedVoxel.x, currentSelectedVoxel.y, currentSelectedVoxel.z)).breakSoundHashes);
                Chunk.SetVoxel(currentSelectedChunk, currentSelectedVoxel.x, currentSelectedVoxel.y, currentSelectedVoxel.z, null);
                currentSelectedChunk.UpdateChunk();
            }
            else if(currentSelectedFaceChunk != null && GetPlacingButtonDown()){
                Chunk.SetVoxelSafe(currentSelectedFaceChunk, currentSelectedFaceVoxel.x, currentSelectedFaceVoxel.y, currentSelectedFaceVoxel.z, VoxelContainer.GetVoxel("Std_Iron_Block"));
                SoundContainer.PlayMultiGlobalSFX(VoxelContainer.GetVoxel("Std_Iron_Block").placeSoundHashes);
                currentSelectedFaceChunk.UpdateChunk();
            }
        }

        if(GetSaveButtonDown())
            world.SaveWorld();

        switch(state){
            case State.Idling:
                Looking();
                if(!IsInputVectorZero())
                    state = State.Flying;
            break;
            case State.Flying:
                Looking();
                Accelerate();
                Flying();
                lastInputVector = smoothInputVector;
                if(IsInputVectorZero())
                    state = State.SlowingDown;
            break;
            case State.SlowingDown:
                Looking();
                Decelerate();
                SlowingDown();
                if(uniformSpeed <= 0.01f)
                    state = State.Idling;
                else if(!IsInputVectorZero())
                    state = State.Flying;
            break;
        }
    }

    #region Gameplay Gizmos
    private void OnPostRender() {
        if(currentSelectedChunk != null)
            DrawBlockSelectionGizmo(currentSelectedVoxel + currentSelectedChunk.transform.position);
    }
    private void DrawBlockSelectionGizmo(Vector3 cornerPoint){
        GL.PushMatrix();
        gizmoMaterial.SetPass(0);
        GL.Begin(GL.LINES);
        GL.Color(Color.black);
        
        //Bottom
        GL.Vertex(cornerPoint);
        GL.Vertex(cornerPoint + Vector3.right);
        GL.Vertex(cornerPoint);
        GL.Vertex(cornerPoint + Vector3.forward);
        GL.Vertex(cornerPoint + new Vector3(1.0f, 0.0f, 1.0f));
        GL.Vertex(cornerPoint + Vector3.right);
        GL.Vertex(cornerPoint + new Vector3(1.0f, 0.0f, 1.0f));
        GL.Vertex(cornerPoint + Vector3.forward);

        //Top
        GL.Vertex(cornerPoint + Vector3.up);
        GL.Vertex(cornerPoint + Vector3.right + Vector3.up);
        GL.Vertex(cornerPoint + Vector3.up);
        GL.Vertex(cornerPoint + Vector3.forward + Vector3.up);
        GL.Vertex(cornerPoint + Vector3.one);
        GL.Vertex(cornerPoint + Vector3.right + Vector3.up);
        GL.Vertex(cornerPoint + Vector3.one);
        GL.Vertex(cornerPoint + Vector3.forward + Vector3.up);

        //Sides
        GL.Vertex(cornerPoint);
        GL.Vertex(cornerPoint + Vector3.up);
        GL.Vertex(cornerPoint + Vector3.right);
        GL.Vertex(cornerPoint + Vector3.right + Vector3.up);
        GL.Vertex(cornerPoint + Vector3.forward);
        GL.Vertex(cornerPoint + Vector3.forward + Vector3.up);
        GL.Vertex(cornerPoint + new Vector3(1.0f, 0.0f, 1.0f));
        GL.Vertex(cornerPoint + Vector3.one);

        GL.End();
        GL.PopMatrix();
    }
    #endregion

    #region Input
    float GetRotX(){
        float r = 0.0f;
        if(Mouse.current != null)
            r += Mouse.current.delta.x.ReadValue();
        if(Gamepad.current != null)
            r += Gamepad.current.rightStick.x.ReadValue() * 20.0f;
        return r;
    }
    float GetRotY(){
        float r = 0.0f;
        if(Mouse.current != null)
            r += Mouse.current.delta.y.ReadValue();
        if(Gamepad.current != null)
            r += Gamepad.current.rightStick.y.ReadValue() * 20.0f;
        return r;
    }
    Vector3 GetInputVector(){
        Vector3 inputVector = Vector3.zero;
        if(Keyboard.current != null){
            if(Keyboard.current.dKey.isPressed)
                inputVector.x += 1.0f;
            if(Keyboard.current.aKey.isPressed)
                inputVector.x -= 1.0f;
            if(Keyboard.current.wKey.isPressed)
                inputVector.z += 1.0f;
            if(Keyboard.current.sKey.isPressed)
                inputVector.z -= 1.0f;
        }
        if(Gamepad.current != null){
            inputVector.x += Gamepad.current.leftStick.x.ReadValue();
            inputVector.z += Gamepad.current.leftStick.y.ReadValue();
        }
        return inputVector.normalized;
    }

    bool IsInputVectorZero(){
        return inputVector.x == 0.0f && inputVector.z == 0.0f;
    }

    bool GetMiningButtonDown(){
        bool r = false;
        if(Mouse.current != null)
            if(Mouse.current.leftButton.wasPressedThisFrame)
                r = true;
        if(Gamepad.current != null)
            if(Gamepad.current.rightShoulder.wasPressedThisFrame)
                r = true;
        return r;
    }
    bool GetPlacingButtonDown(){
        bool r = false;
        if(Mouse.current != null)
            if(Mouse.current.rightButton.wasPressedThisFrame)
                r = true;
        if(Gamepad.current != null)
            if(Gamepad.current.leftShoulder.wasPressedThisFrame)
                r = true;
        return r;
    }

    bool GetSaveButtonDown(){
        bool r = false;
        if(Keyboard.current != null)
            if(Keyboard.current.enterKey.wasPressedThisFrame)
                r = true;
        return r;
    }
    #endregion

    #region Actions
    void Looking(){
        camRotX -= GetRotY() * sensitivity * 0.01f;
        camRotX = Mathf.Clamp(camRotX, minCamRot, maxCamRot);
        camRotY += GetRotX() * sensitivity * 0.01f;
        sRotX = Mathf.Lerp(sRotX, camRotX, Time.deltaTime * camSmoothing);
        sRotY = Mathf.Lerp(sRotY, camRotY, Time.deltaTime * camSmoothing);

        transform.rotation = Quaternion.identity;
        transform.Rotate(Vector3.right * camRotX, Space.World);
        transform.Rotate(Vector3.up * camRotY, Space.World);
    }

    void Accelerate(){
        uniformSpeed = Mathf.Lerp(uniformSpeed, 1.0f, Time.deltaTime * acceleration);
    }
    void Decelerate(){
        uniformSpeed = Mathf.Lerp(uniformSpeed, 0.0f, Time.deltaTime * deceleration);
    }

    void Flying(){
        transform.Translate(smoothInputVector * Time.deltaTime * uniformSpeed * speed, Space.Self);
    }
    void SlowingDown(){
        transform.Translate(lastInputVector * Time.deltaTime * uniformSpeed * speed, Space.Self);
    }
    #endregion
}
