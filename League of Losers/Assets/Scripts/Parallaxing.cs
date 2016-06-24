using UnityEngine;
using System.Collections;

/// <summary>
/// Script gérant l'effet de parallax du background
/// </summary>

public class Parallaxing : MonoBehaviour {

	public Transform[] backgrounds;			// liste contenant tous les backgrounds qui doivent recevoir l'effet
	private float[] parallaxScales;			// la proportion du mouvement de la caméra à appliquer au mouvement du background
	public float smoothing = 1f;			// lissage du parallax
    public float vertMov = 1f;              // vitesse verticale du parallax

	private Transform cam;					// référence à la caméra principale
	private Vector3 previousCamPos;			// position de la caméra à la frame précédente

    /// <summary>
    /// Première initialisation, récupère la caméra
    /// </summary>
	void Awake () {
		cam = Camera.main.transform;
    }

    /// <summary>
    /// Initialisation, récupère les différents composants et assigne les parallax
    /// </summary>
	void Start () {


		// on assigne les parallax sur les backgrounds
		parallaxScales = new float[backgrounds.Length];
		for (int i = 0; i < backgrounds.Length; i++) {
			parallaxScales[i] = backgrounds[i].position.z*-1;
		}

        // on récupère la position de la caméra
        previousCamPos = cam.position;
    }

    /// <summary>
    /// Appelé à chaque frame. Agit sur les backgrounds en fonction du mouvement de la caméra entre cette frame et la précédente
    /// </summary>
    void Update () {

		for (int i = 0; i < backgrounds.Length; i++) {
            // on définit le parallax horizontal et vertical
			float parallax = (previousCamPos.x - cam.position.x) * parallaxScales[i];
            float parallaxY = (previousCamPos.y - cam.position.y) * parallaxScales[i] * vertMov;

            // on définit la nouvelle position du background visé, qui est sa position initiale + le parallax horizontal et vertical
            float backgroundTargetPosX = backgrounds[i].position.x + parallax;
            float backgroundTargetPosY = backgrounds[i].position.y + parallaxY;

            // on applique la nouvelle position du background
            Vector3 backgroundTargetPos = new Vector3 (backgroundTargetPosX, backgroundTargetPosY, backgrounds[i].position.z);

            // transition entre l'ancienne et la nouvelle position en utilisant lerp et le lissage
			backgrounds[i].position = Vector3.Lerp (backgrounds[i].position, backgroundTargetPos, smoothing * Time.deltaTime);
		}

        // on récupère la position de la caméra pour l'utiliser dans les calculs de la prochaine frame
		previousCamPos = cam.position;
	}
}
