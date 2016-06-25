using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// Gère l'affichage des joueurs (perdants et victorieux) dans le panneau des scores.
/// </summary>
public class WinPanel : MonoBehaviour {
    
    public Animator anim;
    public Text WinnerName;
    public Text LoserName;
    public List<Text> OtherPlayerNames;
    bool shown = false;

    /// <summary>
    /// Use this for initialization
    /// </summary>
    void Start () {
        anim.SetBool("Shown", false);
    }
    
    /// <summary>
    /// Supprime tout les noms de joueurs contenu dans le panneau
    /// </summary>
    public void Clear()
    {
        WinnerName.text = "Personne";
        LoserName.text = "Personne";
        foreach (Text text in OtherPlayerNames)
            text.text = "Personne";
    }
    
    /// <summary>
    /// Définit le joueur gagnant.
    /// </summary>
    /// <param name="name">nom du joueur</param>
    /// <param name="score">score du joueur</param>
    public void setWinner(string name, int score)
    {
        Debug.Log("Winner: " + name);
        WinnerName.text = name + ", " + score;
    }
    
    /// <summary>
    /// Définit le joueur perdant (loser en chef).
    /// </summary>
    /// <param name="name">nom du joueur</param>
    /// <param name="score">score du joueur</param>
    public void setLoser(string name, int score)
    {
        Debug.Log("Loser: " + name);
        LoserName.text = name + ", " + score;
    }
    
    
    /// <summary>
    /// Définit le joueur 2.
    /// </summary>
    /// <param name="name">nom du joueur</param>
    /// <param name="score">score du joueur</param>
    public void setPlayer2(string name, int score)
    {
        OtherPlayerNames[0].text = name + ", " + score;
    }
    
    /// <summary>
    /// Définit le joueur 3.
    /// </summary>
    /// <param name="name">nom du joueur</param>
    /// <param name="score">score du joueur</param>
    public void setPlayer3(string name, int score)
    {
        OtherPlayerNames[1].text = name + ", " + score;
    }
    
    /// <summary>
    /// Affiche le panneau
    /// </summary>
    public void show()
    {
        Debug.Log("Showing");
        anim.SetBool("Shown", true);
        shown = true;
    }
    
    /// <summary>
    /// Masque le panneau
    /// </summary>
    public void hide()
    {
        Debug.Log("Hiding");
        anim.SetBool("Shown", false);
        shown = false;
    }
    
    /// <summary>
    /// Retourne au menu principal suite à un clic
    /// </summary>
    public void ReturnToMainMenu()
    {
        if (shown)
        {
            string[] listePropriete = new string[] { "Classe", "Couleur" };
            PhotonNetwork.RemovePlayerCustomProperties(listePropriete);
            PhotonNetwork.Disconnect();
            SceneManager.LoadScene(0);
        }
            
    }
}
