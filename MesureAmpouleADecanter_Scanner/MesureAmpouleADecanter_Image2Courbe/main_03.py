import cv2
import numpy as np
import matplotlib

matplotlib.use('TkAgg')  # Pour afficher dans des fenêtres séparées
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

def extraire_timespan(chemin):
    import re
    match = re.search(r'\[([0-9,\.]+)s\]', chemin)
    if match:
        timespan_str = match.group(1).replace(',', '.')
        return float(timespan_str)
    return None

def extraire_courbe_decantation(chemin_image, seuil_detection=0.7, sens='haut_vers_bas',
                                lisser=True, methode_lissage='savitzky_golay', verbose=False, **kwargs_lissage):
    """
    Extrait la courbe de décantation d'une image temporelle.

    Args:
        chemin_image: Chemin vers l'image
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

    timespan = extraire_timespan(chemin_image)

    if img is None:
        raise ValueError(f"Impossible de charger l'image: {chemin_image}")

    hauteur, largeur = img.shape
    img_norm = img.astype(float) / 255.0

    pasX = timespan / largeur

    if verbose:
        print(f"Image shape: {img.shape}")
        print(
            f"Valeurs normalisées - Min: {img_norm.min():.3f}, Max: {img_norm.max():.3f}, Mean: {img_norm.mean():.3f}")

    temps = []
    temps_t = []
    hauteurs = []

    # Parcourir chaque colonne pixel par pixel (pas = 1)
    for x in range(largeur):
        colonne_norm = img_norm[:, x]
        t = x * pasX

        if verbose and x % 100 == 0:  # Afficher progression tous les 100 pixels
            print(f"  Traitement colonne {x}/{largeur}")

        if sens == 'haut_vers_bas':
            idx_front = None
            # Parcourir du haut vers le bas
            for y in range(hauteur):
                if verbose and x % 500 == 0 and y % 100 == 0:  # Debug détaillé
                    print(f"    x={x}, y={y}, valeur={colonne_norm[y]:.3f}, seuil={seuil_detection}")

                if colonne_norm[y] < seuil_detection:
                    idx_front = y
                    if verbose and x % 500 == 0:
                        print(f"    -> Front détecté à y={y}")
                    break

            if idx_front is None:
                idx_front = hauteur - 1
                if verbose and x % 500 == 0:
                    print(f"    -> Aucun front détecté, utilisation de y={idx_front}")

        elif sens == 'bas_vers_haut':
            idx_front = None
            # Parcourir du bas vers le haut
            for y in range(hauteur - 1, -1, -1):
                if verbose and x % 500 == 0 and y % 100 == 0:  # Debug détaillé
                    print(f"    x={x}, y={y}, valeur={colonne_norm[y]:.3f}, seuil={seuil_detection}")

                if colonne_norm[y] < seuil_detection:
                    idx_front = y
                    if verbose and x % 500 == 0:
                        print(f"    -> Front détecté à y={y}")
                    break

            if idx_front is None:
                idx_front = 0
                if verbose and x % 500 == 0:
                    print(f"    -> Aucun front détecté, utilisation de y={idx_front}")

        else:
            raise ValueError("Le paramètre 'sens' doit être 'haut_vers_bas' ou 'bas_vers_haut'")

        temps.append(x)
        temps_t.append(t)
        hauteurs.append(idx_front)

    if verbose:
        print(f"Nombre de points extraits: {len(temps)}")
        print(f"Hauteurs - Min: {min(hauteurs)}, Max: {max(hauteurs)}")

    # Convertir en arrays NumPy AVANT l'inversion
    temps = np.array(temps)
    hauteurs = np.array(hauteurs)
    temps_t = np.array(temps_t)

    # Inverser les hauteurs : le point le plus bas (Y max) devient 0
    hauteurs = hauteur - hauteurs

    if lisser:
        hauteurs_lissees = lisser_courbe(hauteurs, methode=methode_lissage, **kwargs_lissage)
        return temps, hauteurs, hauteurs_lissees, temps_t

    return temps, hauteurs, hauteurs, temps_t


def visualiser_resultat(chemin_image, temps, temps_t, hauteurs, hauteurs_lissees=None,
                        facteur_cm_par_pixel=0.1, sauvegarder=False, dossier_sortie=None):
    """Visualise l'image et la courbe extraite avec hauteurs en cm."""
    img = cv2.imread(chemin_image)
    hauteur_img = img.shape[0]

    # Conversion en cm
    hauteurs_cm = hauteurs * facteur_cm_par_pixel
    hauteurs_lissees_cm = hauteurs_lissees * facteur_cm_par_pixel if hauteurs_lissees is not None else None

    # Inverser pour l'affichage sur l'image (coordonnées image)
    hauteurs_affichage = hauteur_img - hauteurs
    hauteurs_lissees_affichage = hauteur_img - hauteurs_lissees if hauteurs_lissees is not None else None

    fig, (ax1, ax2) = plt.subplots(2, 1, figsize=(12, 10))

    # Graphique 1: Image avec courbe superposée (en pixels)
    ax1.imshow(cv2.cvtColor(img, cv2.COLOR_BGR2RGB))
    ax1.plot(temps, hauteurs_affichage, 'r-', linewidth=1, alpha=0.5, label='Front brut')
    if hauteurs_lissees_affichage is not None:
        ax1.plot(temps, hauteurs_lissees_affichage, 'g-', linewidth=2, label='Front lissé')
    ax1.set_xlabel('Temps (sec)', fontsize=11)
    ax1.set_ylabel('Position Y (pixels)', fontsize=11)
    ax1.set_title(f'Image avec courbe détectée - {Path(chemin_image).name}', fontsize=12, fontweight='bold')
    ax1.legend()
    ax1.grid(True, alpha=0.3)

    # Graphique 2: Courbe en cm (avec 0 en bas)
    ax2.plot(temps_t, hauteurs_cm, 'b-o', linewidth=1, markersize=3, alpha=0.5, label='Courbe brute')
    if hauteurs_lissees_cm is not None:
        ax2.plot(temps_t, hauteurs_lissees_cm, 'r-', linewidth=2, label='Courbe lissée')
    ax2.set_xlabel('Temps (s)', fontsize=11)
    ax2.set_ylabel('Hauteur (cm)', fontsize=11)
    ax2.set_title(f'Courbe de décantation (facteur: {facteur_cm_par_pixel} cm/px)', fontsize=12, fontweight='bold')
    ax2.grid(True, alpha=0.3)
    ax2.legend()

    # Ajouter des statistiques
    if hauteurs_lissees_cm is not None:
        hauteur_min = hauteurs_lissees_cm.min()
        hauteur_max = hauteurs_lissees_cm.max()
        hauteur_moy = hauteurs_lissees_cm.mean()
        ax2.text(0.02, 0.98,
                 f'Min: {hauteur_min:.2f} cm\nMax: {hauteur_max:.2f} cm\nMoy: {hauteur_moy:.2f} cm',
                 transform=ax2.transAxes,
                 verticalalignment='top',
                 bbox=dict(boxstyle='round', facecolor='wheat', alpha=0.5),
                 fontsize=9)

    plt.tight_layout()

    if sauvegarder and dossier_sortie:
        nom_fichier = Path(chemin_image).stem
        chemin_sortie = os.path.join(dossier_sortie, f'{nom_fichier}_courbe.png')
        plt.savefig(chemin_sortie, dpi=150, bbox_inches='tight')
        print(f"  Graphique sauvegardé: {chemin_sortie}")

    plt.show(block=False)
    plt.pause(0.1)


def visualiser_comparaison_multiple(resultats, facteur_cm_par_pixel=0.1,
                                    sauvegarder=False, dossier_sortie=None):
    """Visualise toutes les courbes sur un même graphique."""

    fig, (ax1, ax2) = plt.subplots(2, 1, figsize=(14, 10))

    couleurs = plt.cm.tab10(np.linspace(0, 1, len(resultats)))

    for i, (nom_image, donnees) in enumerate(resultats.items()):
        if 'erreur' in donnees:
            continue

        temps = donnees['temps']
        hauteurs_cm = donnees['hauteurs'] * facteur_cm_par_pixel
        hauteurs_lissees_cm = donnees['hauteurs_lissees'] * facteur_cm_par_pixel

        # Graphique 1: Courbes brutes
        ax1.plot(temps, hauteurs_cm, '-', linewidth=1, alpha=0.6,
                 color=couleurs[i], label=nom_image)

        # Graphique 2: Courbes lissées
        ax2.plot(temps, hauteurs_lissees_cm, '-', linewidth=2,
                 color=couleurs[i], label=nom_image)

    ax1.set_xlabel('Temps (pixels)', fontsize=11)
    ax1.set_ylabel('Hauteur (cm)', fontsize=11)
    ax1.set_title('Comparaison des courbes brutes', fontsize=12, fontweight='bold')
    ax1.grid(True, alpha=0.3)
    ax1.legend(bbox_to_anchor=(1.05, 1), loc='upper left', fontsize=8)

    ax2.set_xlabel('Temps (pixels)', fontsize=11)
    ax2.set_ylabel('Hauteur (cm)', fontsize=11)
    ax2.set_title(f'Comparaison des courbes lissées (facteur: {facteur_cm_par_pixel} cm/px)',
                  fontsize=12, fontweight='bold')
    ax2.grid(True, alpha=0.3)
    ax2.legend(bbox_to_anchor=(1.05, 1), loc='upper left', fontsize=8)

    plt.tight_layout()

    if sauvegarder and dossier_sortie:
        chemin_sortie = os.path.join(dossier_sortie, 'comparaison_toutes_courbes.png')
        plt.savefig(chemin_sortie, dpi=150, bbox_inches='tight')
        print(f"\nGraphique de comparaison sauvegardé: {chemin_sortie}")

    plt.show(block=False)
    plt.pause(0.1)


def convertir_en_cm(hauteurs_pixels, facteur_conversion):
    """Convertit les hauteurs de pixels en centimètres."""
    return hauteurs_pixels * facteur_conversion


def traiter_dossier(dossier_images, dossier_sortie='resultats',
                    seuil_detection=0.7, sens='haut_vers_bas',
                    lisser=True, methode_lissage='savitzky_golay',
                    facteur_cm_par_pixel=0.1,
                    afficher_graphiques=True,
                    sauvegarder_graphiques=True,
                    afficher_comparaison=True,
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
        afficher_graphiques: Afficher les graphiques individuels
        sauvegarder_graphiques: Sauvegarder les graphiques
        afficher_comparaison: Afficher un graphique de comparaison (True/False)
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
    print(f"Facteur de conversion: {facteur_cm_par_pixel} cm/pixel")
    print(f"{'=' * 60}\n")

    resultats = {}

    for i, chemin_image in enumerate(fichiers_images, 1):
        nom_fichier = chemin_image.name
        print(f"[{i}/{len(fichiers_images)}] Traitement: {nom_fichier}")

        # try:
        # Extraire la courbe
        temps, hauteurs, hauteurs_lissees, temps_t = extraire_courbe_decantation(
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
                   np.column_stack([temps_t, hauteurs, hauteurs_lissees, hauteurs_cm]),
                   delimiter=',',
                   header='Temps(s),Hauteur_brute(px),Hauteur_lissee(px),Hauteur(cm)',
                   comments='')
        print(f"  CSV sauvegardé: {chemin_csv}")

        # Visualiser
        if afficher_graphiques or sauvegarder_graphiques:
            visualiser_resultat(
                str(chemin_image),
                temps,
                temps_t,
                hauteurs,
                hauteurs_lissees,
                facteur_cm_par_pixel=facteur_cm_par_pixel,
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

        print(f"  ✓ Traitement réussi (hauteur: {hauteurs_cm.min():.2f}-{hauteurs_cm.max():.2f} cm)\n")

        # except Exception as e:
        #     print(f"  ✗ Erreur: {e}\n")
        #     resultats[nom_fichier] = {'erreur': str(e)}

    print(f"\n{'=' * 60}")
    print(
        f"Traitement terminé: {len([r for r in resultats.values() if 'erreur' not in r])}/{len(fichiers_images)} réussi(s)")
    print(f"Résultats sauvegardés dans: {dossier_sortie}")
    print(f"{'=' * 60}\n")

    # Afficher le graphique de comparaison (optionnel)
    if afficher_comparaison and len([r for r in resultats.values() if 'erreur' not in r]) > 1:
        visualiser_comparaison_multiple(
            resultats,
            facteur_cm_par_pixel=facteur_cm_par_pixel,
            sauvegarder=sauvegarder_graphiques,
            dossier_sortie=dossier_sortie
        )

    return resultats


#pour fichier de 15h28 à 15h46 : seuil_grignon = 0.1, seuil_boue= 0.8
#pour fichier de 15h57 à 16h14 : seuil_grignon = 0.1 - 0.3
#pour fichier de 16h24 à 16h23 : seuil_grignon = 0.7
#pour fichier de 16h29 à 16h34 : seuil_grignon = 0.2

if __name__ == "__main__":
    # Traiter tout un dossier
    resultats = traiter_dossier(
        dossier_images='images',  # Dossier contenant vos images
        dossier_sortie='resultats',  # Dossier de sortie
        seuil_detection=0.5,
        sens='haut_vers_bas',
        lisser=True,
        methode_lissage='savitzky_golay',
        window_length=15,
        polyorder=3,
        facteur_cm_par_pixel=40.0/160,  # Ajustez selon votre calibration
        afficher_graphiques=True,  # True pour afficher, False pour batch
        sauvegarder_graphiques=True,
        afficher_comparaison=True  # True/False pour afficher la synthèse
    )

    # Garder les fenêtres ouvertes
    plt.show()
