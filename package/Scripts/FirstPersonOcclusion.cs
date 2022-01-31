using System.Collections.Generic;
using UnityEngine;

public class FirstPersonOcclusion : MonoBehaviour
{
   
    public AudioSource[] Audios;
   
    public AudioListener Listener;

    [Header("Occlusion Options")]
    [Range(0f, 10f)]
    public float SoundOcclusionWidening = 1f;
    
    private float PlayerOcclusionWidening = .2f;
    public LayerMask OcclusionLayer;

    public float MaxDistance;
    public float ListenerDistance;
    private float lineCastHitCount = 0f;
    public float lineCastHitCountDisplay = 0f;

    private Color colour;

    private float maxPitchSubtraction = 0.0f;
    public float maxVolumeSubtraction = 0.5f;

    private float lerpTime = 15f;

    public bool DebugSound = false;

    public List<Collider> collidersToIgnore = new List<Collider>();

    private void Start()
    {
        Audios = GetComponents<AudioSource>();
        MaxDistance = Audios[0].maxDistance;
    }

    private void Update()
    {
        if(!Listener)
        {
            Listener = GameObject.FindObjectOfType<AudioListener>();
        }
        if(Audios.Length == 0)
        Audios = GetComponents<AudioSource>();

   
        if(DebugSound)
        Debug.Log("Listener " + Listener + "  Audios.Length" + Audios.Length + " Audios[0].isPlaying" + Audios[0].isPlaying);
        if (Listener && Audios.Length>0 && Audios[0].isPlaying)
        {
            ListenerDistance = Vector3.Distance(transform.position, Listener.transform.position);

            if (ListenerDistance <= MaxDistance)
                OccludeBetween(transform.position, Listener.transform.position);

            lineCastHitCount = 0f;
        }
    }
    Vector3 SoundLeft;// = CalculatePoint(sound, listener, SoundOcclusionWidening, true);
    Vector3 SoundRight;// = CalculatePoint(sound, listener, SoundOcclusionWidening, false);

    Vector3 SoundAbove;// = new Vector3(sound.x, sound.y + SoundOcclusionWidening, sound.z);
    Vector3 SoundBelow;// = new Vector3(sound.x, sound.y - SoundOcclusionWidening, sound.z);

    Vector3 ListenerLeft;// = CalculatePoint(listener, sound, PlayerOcclusionWidening, true);
    Vector3 ListenerRight;// = CalculatePoint(listener, sound, PlayerOcclusionWidening, false);

    Vector3 ListenerAbove;// = new Vector3(listener.x, listener.y + PlayerOcclusionWidening * 0.5f, listener.z);
    Vector3 ListenerBelow;// = new Vector3(listener.x, listener.y - PlayerOcclusionWidening * 0.5f, listener.z);
    private void OccludeBetween(Vector3 sound, Vector3 listener)
    {
         SoundLeft = CalculatePoint(sound, listener, SoundOcclusionWidening, true);
         SoundRight = CalculatePoint(sound, listener, SoundOcclusionWidening, false);

         SoundAbove = new Vector3(sound.x, sound.y + SoundOcclusionWidening, sound.z);
         SoundBelow = new Vector3(sound.x, sound.y - SoundOcclusionWidening, sound.z);

         ListenerLeft = CalculatePoint(listener, sound, PlayerOcclusionWidening, true);
         ListenerRight = CalculatePoint(listener, sound, PlayerOcclusionWidening, false);

         ListenerAbove = new Vector3(listener.x, listener.y + PlayerOcclusionWidening * 0.5f, listener.z);
         ListenerBelow = new Vector3(listener.x, listener.y - PlayerOcclusionWidening * 0.5f, listener.z);

        CastLine(SoundLeft, ListenerLeft);
        CastLine(SoundLeft, listener);
        CastLine(SoundLeft, ListenerRight);

        CastLine(sound, ListenerLeft);
        CastLine(sound, listener);
        CastLine(sound, ListenerRight);

        CastLine(SoundRight, ListenerLeft);
        CastLine(SoundRight, listener);
        CastLine(SoundRight, ListenerRight);

        CastLine(SoundAbove, ListenerAbove);
        CastLine(SoundBelow, ListenerBelow);

        if (PlayerOcclusionWidening == 0f || SoundOcclusionWidening == 0f)
        {
            colour = Color.blue;
        }
        else
        {
            colour = Color.green;
        }

        SetParameter();
    }

    float x;
    float z;
    float n;
    float mn;

    private Vector3 CalculatePoint(Vector3 a, Vector3 b, float m, bool posOrneg)
    {
         n = Vector3.Distance(new Vector3(a.x, 0f, a.z), new Vector3(b.x, 0f, b.z));
         mn = (m / n);
        if (posOrneg)
        {
            x = a.x + (mn * (a.z - b.z));
            z = a.z - (mn * (a.x - b.x));
        }
        else
        {
            x = a.x - (mn * (a.z - b.z));
            z = a.z + (mn * (a.x - b.x));
        }
        return new Vector3(x, a.y, z);
    }

    private RaycastHit[] hits;
    private bool validHit;
    private void CastLine(Vector3 Start, Vector3 End)
    {
        //
       validHit = false;
       
        hits = Physics.RaycastAll(Start, (End - Start).normalized, Vector3.Distance(Start, End), OcclusionLayer);

        // Loop through the array containing the information
        // describing the "collision" between the ray and the hit object
        for (int i = 0; i < hits.Length; i++)
        {
           
            if (!collidersToIgnore.Contains(hits[i].collider))
            {
                validHit = true;

                //Debug.DrawLine(Start, hits[i].point, Color.white);


                break;
            }
           

            // Here, you can access the information of the hit object using hit.collider or hit.transform
        }
        if(validHit)
        {
            lineCastHitCount++;
           // Debug.DrawLine(Start, End, Color.red);
        }
        else
        {
//Debug.DrawLine(Start, End, colour);
        }


        //


        /*


        RaycastHit hit;
        Physics.Linecast(Start, End, out hit, OcclusionLayer);

        if (hit.collider)
        {
            lineCastHitCount++;
            Debug.DrawLine(Start, End, Color.red);
        }
        else
            Debug.DrawLine(Start, End, colour);
        */
    }

    private void SetParameter()
    {
        
        lineCastHitCountDisplay = lineCastHitCount;
        for (int i = 0; i < Audios.Length; i++)
        {
            Audios[i].volume = Mathf.Lerp(Audios[i].volume, (1f - (maxVolumeSubtraction * (lineCastHitCount / 11))), Time.deltaTime* lerpTime);

            Audios[i].pitch = Mathf.Lerp(Audios[i].pitch, (1 - (maxPitchSubtraction * (lineCastHitCount / 11))), Time.deltaTime * lerpTime);

        }
      //  if (DebugSound)
        //    Debug.Log("Setting volume"+ (1f - (maxVolumeSubtraction * (lineCastHitCount / 11))));
    }
}