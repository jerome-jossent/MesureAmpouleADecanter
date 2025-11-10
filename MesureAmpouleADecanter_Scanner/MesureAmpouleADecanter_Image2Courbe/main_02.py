import cv2
import numpy as np
import matplotlib.pyplot as plt
from scipy.signal import savgol_filter
from scipy.ndimage import gaussian_filter1d
from scipy.interpolate import UnivariateSpline
import os
from pathlib import Path


def lisser_courbe(hauteurs, methode='savitzky_golay', **kwargs):
    """
    Lisse la courbe de décantation.

    Args:
        hauteurs: Array des hauteurs à lisser
        methode: Type de lissage ('savitzky_golay', 'gaussian', 'moyenne_mobile', 'spline')
        **kwargs: Paramètres spécifiques à chaque méthode

    Returns:
        hauteurs_lissees: Array des hauteurs lissées
    """

    if methode == 'savitzky_golay':
        window_length = kwargs.get('window_length', 11)
        polyorder = kwargs.get('polyorder', 3)

        if window_length % 2 == 0:
            window_length += 1
        window_length = min(window_length, len(hauteurs) - 1)
        if window_length % 2 == 0:
            window_length -= 1

        return savgol_filter(hauteurs, window_length, polyorder)

    elif methode == 'gaussian':
        sigma = kwargs.get('sigma', 2)
        return gaussian_filter1d(hauteurs, sigma)

    elif methode == 'moyenne_mobile':
        window_size = kwargs.get('window_size', 5)
        return np.convolve(hauteurs, np.ones(window_size) / window_size, mode='same')

    elif methode == 'spline':
        s = kwargs.get('s', len(hauteurs))
        k = kwargs.get('k', 3)
        x = np.arange(len(hauteurs))
        spline = UnivariateSpline(x, hauteurs, s=s, k=k)
        return spline(x)

    else:
        raise ValueError(f"Méthode '{methode}' non reconnue")


def extraire_courbe_decantation(chemin_image, nb_points=30, seuil_detection=0.7, sens='haut_vers_bas',
                                lisser=True, methode_lissage='savitzky_golay', verbose=False, **kwargs_lissage):
    """
    Extrait la courbe de décantation d'une image temporelle.

    Args:
        chemin_image: Chemin vers l'image
        nb_points: Nombre de points à extraire (défaut: 30)
        seuil_detection: Seuil pour détecter le front (0-1, défaut: 0.7)
        sens: Direction de parcours ('haut_vers_bas' ou 'bas_vers_haut')
        lisser: Appliquer un lissage (défaut: True)
        methode_lissage: Méthode de lissage à utiliser
        verbose: Afficher les informations de debug
        **kwargs_lissage: Paramètres pour la méthode de lissage

    Returns:
        temps: Array des positions temporelles (en pixels)
        hauteurs: Array des hauteurs du front (en pixels)
        hauteurs_lissees: Array des hauteurs lissées (si lisser=True)
    """
    img = cv2.imread(chemin_image, cv2.IMREAD_GRAYSCALE)

    if img is None:
        raise ValueError(f"Impossible de charger l'image: {chemin_image}")

    hauteur, largeur = img.shape
    nb_points = largeur
    img_norm = img.astype(float) / 255.0

    if verbose:
        print(f"Image shape: {img.shape}")
        print(
            f"Valeurs normalisées - Min: {img_norm.min():.3f}, Max: {img_norm.max():.3f}, Mean: {img_norm.mean():.3f}")

    pas = max(1, largeur // nb_points)
    temps = []
    hauteurs = []

    for x in range(0, largeur, pas):
        colonne_norm = img_norm[:, x]

        if sens == 'haut_vers_bas':
            idx_front = None
            for y in range(hauteur):
                if colonne_norm[y] < seuil_detection:
                    idx_front = y
                    break
            if idx_front is None:
                idx_front = hauteur - 1

        elif sens == 'bas_vers_haut':
            idx_front = None
            for y in range(hauteur - 1, -1, -1):
                if colonne_norm[y] < seuil_detection:
                    idx_front = y
                    break
            if idx_front is None:
                idx_front = 0
        else:
            raise ValueError("Le paramètre 'sens' doit être 'haut_vers_bas' ou 'bas_vers_haut'")

        temps.append(x)
        hauteurs.append(idx_front)

    if verbose:
        print(f"Nombre de points extraits: {len(temps)}")
        print(f"Hauteurs - Min: {min(hauteurs)}, Max: {max(hauteurs)}")

    temps = np.array(temps)
    hauteurs = np.array(hauteurs)

    if lisser:
        hauteurs_lissees = lisser_courbe(hauteurs, methode=methode_lissage, **kwargs_lissage)
        return temps, hauteurs, hauteurs_lissees

    return temps, hauteurs, hauteurs


def visualiser_resultat(chemin_image, temps, hauteurs, hauteurs_lissees=None, sauvegarder=False, dossier_sortie=None):
    """Visualise l'image et la courbe extraite."""
    img = cv2.imread(chemin_image)

    fig, (ax1, ax2) = plt.subplots(2, 1, figsize=(10, 10))

    ax1.imshow(cv2.cvtColor(img, cv2.COLOR_BGR2RGB))
    ax1.plot(temps, hauteurs, 'r-', linewidth=1, alpha=0.5, label='Front brut')
    if hauteurs_lissees is not None:
        ax1.plot(temps, hauteurs_lissees, 'g-', linewidth=2, label='Front lissé')
    ax1.set_xlabel('Temps (pixels)')
    ax1.set_ylabel('Hauteur (pixels)')
    ax1.set_title(f'Image avec courbe détectée - {Path(chemin_image).name}')
    ax1.legend()

    ax2.plot(temps, hauteurs, 'b-o', linewidth=1, markersize=3, alpha=0.5, label='Courbe brute')
    if hauteurs_lissees is not None:
        ax2.plot(temps, hauteurs_lissees, 'r-', linewidth=2, label='Courbe lissée')
    ax2.set_xlabel('Temps (pixels)')
    ax2.set_ylabel('Hauteur (pixels)')
    ax2.set_title('Courbe de décantation')
    ax2.grid(True)
    ax2.legend()

    plt.tight_layout()

    if sauvegarder and dossier_sortie:
        nom_fichier = Path(chemin_image).stem
        chemin_sortie = os.path.join(dossier_sortie, f'{nom_fichier}_courbe.png')
        plt.savefig(chemin_sortie, dpi=150, bbox_inches='tight')
        print(f"  Graphique sauvegardé: {chemin_sortie}")

    plt.show()
    plt.close()


def convertir_en_cm(hauteurs_pixels, facteur_conversion):
    """Convertit les hauteurs de pixels en centimètres."""
    return hauteurs_pixels * facteur_conversion


def traiter_dossier(dossier_images, dossier_sortie='resultats',
                    seuil_detection=0.7, sens='haut_vers_bas',
                    lisser=True, methode_lissage='savitzky_golay',
                    facteur_cm_par_pixel=0.1,
                    afficher_graphiques=True,
                    sauvegarder_graphiques=True,
                    extensions=['.jpg', '.jpeg', '.png', '.bmp', '.tiff'],
                    **kwargs_lissage):
    """
    Traite toutes les images d'un dossier.

    Args:
        dossier_images: Chemin du dossier contenant les images
        dossier_sortie: Dossier où sauvegarder les résultats
        seuil_detection: Seuil de détection (0-1)
        sens: Direction de parcours
        lisser: Appliquer un lissage
        methode_lissage: Méthode de lissage
        facteur_cm_par_pixel: Facteur de conversion pixels -> cm
        afficher_graphiques: Afficher les graphiques
        sauvegarder_graphiques: Sauvegarder les graphiques
        extensions: Liste des extensions d'images à traiter
        **kwargs_lissage: Paramètres de lissage

    Returns:
        resultats: Dictionnaire avec les résultats pour chaque image
    """

    # Créer le dossier de sortie
    Path(dossier_sortie).mkdir(parents=True, exist_ok=True)

    # Lister toutes les images
    fichiers_images = []
    for ext in extensions:
        fichiers_images.extend(Path(dossier_images).glob(f'*{ext}'))
        fichiers_images.extend(Path(dossier_images).glob(f'*{ext.upper()}'))

    fichiers_images = sorted(set(fichiers_images))

    if not fichiers_images:
        print(f"Aucune image trouvée dans {dossier_images}")
        return {}

    print(f"\n{'=' * 60}")
    print(f"Traitement de {len(fichiers_images)} image(s) du dossier: {dossier_images}")
    print(f"{'=' * 60}\n")

    resultats = {}

    for i, chemin_image in enumerate(fichiers_images, 1):
        nom_fichier = chemin_image.name
        print(f"[{i}/{len(fichiers_images)}] Traitement: {nom_fichier}")

        try:
            # Extraire la courbe
            temps, hauteurs, hauteurs_lissees = extraire_courbe_decantation(
                str(chemin_image),
                seuil_detection=seuil_detection,
                sens=sens,
                lisser=lisser,
                methode_lissage=methode_lissage,
                verbose=False,
                **kwargs_lissage
            )

            # Conversion en cm
            hauteurs_cm = convertir_en_cm(hauteurs_lissees, facteur_cm_par_pixel)

            # Sauvegarder les données CSV
            nom_base = chemin_image.stem
            chemin_csv = os.path.join(dossier_sortie, f'{nom_base}_courbe.csv')
            np.savetxt(chemin_csv,
                       np.column_stack([temps, hauteurs, hauteurs_lissees, hauteurs_cm]),
                       delimiter=',',
                       header='Temps(px),Hauteur_brute(px),Hauteur_lissee(px),Hauteur(cm)',
                       comments='')
            print(f"  CSV sauvegardé: {chemin_csv}")

            # Visualiser
            if afficher_graphiques or sauvegarder_graphiques:
                visualiser_resultat(
                    str(chemin_image),
                    temps,
                    hauteurs,
                    hauteurs_lissees,
                    sauvegarder=sauvegarder_graphiques,
                    dossier_sortie=dossier_sortie
                )
                if not afficher_graphiques:
                    plt.close('all')

            # Stocker les résultats
            resultats[nom_fichier] = {
                'temps': temps,
                'hauteurs': hauteurs,
                'hauteurs_lissees': hauteurs_lissees,
                'hauteurs_cm': hauteurs_cm,
                'chemin_csv': chemin_csv
            }

            print(f"  ✓ Traitement réussi\n")

        except Exception as e:
            print(f"  ✗ Erreur: {e}\n")
            resultats[nom_fichier] = {'erreur': str(e)}

    print(f"\n{'=' * 60}")
    print(
        f"Traitement terminé: {len([r for r in resultats.values() if 'erreur' not in r])}/{len(fichiers_images)} réussi(s)")
    print(f"Résultats sauvegardés dans: {dossier_sortie}")
    print(f"{'=' * 60}\n")

    return resultats


facteur_cm_par_pixel = 0.1

# Exemple d'utilisation
if __name__ == "__main__":

    # Traiter tout un dossier
    resultats = traiter_dossier(
        dossier_images='images',  # Dossier contenant vos images
        dossier_sortie='resultats',  # Dossier de sortie
        seuil_detection=0.9,
        sens='haut_vers_bas',
        lisser=True,
        methode_lissage='savitzky_golay',
        window_length=15,
        polyorder=3,
        facteur_cm_par_pixel=0.25,
        afficher_graphiques=False,  # True pour afficher, False pour traiter en batch
        sauvegarder_graphiques=True  # Sauvegarder les graphiques
    )

    # Accéder aux résultats
    for nom_image, donnees in resultats.items():
        if 'erreur' not in donnees:
            print(f"{nom_image}: {len(donnees['temps'])} points extraits")
