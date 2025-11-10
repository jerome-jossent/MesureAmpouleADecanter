import cv2
import numpy as np
import matplotlib.pyplot as plt


def extraire_courbe_decantation(chemin_image, nb_points=30, seuil_detection=0.7, sens='haut_vers_bas'):
    """
    Extrait la courbe de décantation d'une image temporelle.

    Args:
        chemin_image: Chemin vers l'image
        nb_points: Nombre de points à extraire (défaut: 30)
        seuil_detection: Seuil pour détecter le front (0-1, défaut: 0.7)
        sens: Direction de parcours ('haut_vers_bas' ou 'bas_vers_haut')

    Returns:
        temps: Array des positions temporelles (en pixels)
        hauteurs: Array des hauteurs du front (en pixels)
    """
    # Charger l'image en niveaux de gris
    img = cv2.imread(chemin_image, cv2.IMREAD_GRAYSCALE)

    if img is None:
        raise ValueError("Impossible de charger l'image")

    hauteur, largeur = img.shape

    nb_points = largeur

    # Normalisation globale de l'image entière (0-1)
    img_norm = img.astype(float) / 255.0

    print(f"Image shape: {img.shape}")
    print(f"Valeurs normalisées - Min: {img_norm.min():.3f}, Max: {img_norm.max():.3f}, Mean: {img_norm.mean():.3f}")

    # Calculer le pas d'échantillonnage
    pas = max(1, largeur // nb_points)

    temps = []
    hauteurs = []

    # Parcourir l'image avec le pas défini
    for x in range(0, largeur, pas):
        colonne_norm = img_norm[:, x]

        # Debug: afficher les valeurs pour quelques colonnes
        if x % (largeur // 5) == 0:  # Afficher pour 5 colonnes espacées
            print(f"\nColonne x={x}:")
            print(f"  Valeurs normalisées - Min: {colonne_norm.min():.3f}, Max: {colonne_norm.max():.3f}")
            print(f"  Premiers pixels: {colonne_norm[:5]}")
            print(f"  Derniers pixels: {colonne_norm[-5:]}")

        # Détection selon le sens de parcours
        if sens == 'haut_vers_bas':
            # Parcourir du haut vers le bas
            idx_front = None
            for y in range(hauteur):
                if colonne_norm[y] < seuil_detection:
                    idx_front = y
                    break
            if idx_front is None:
                idx_front = hauteur - 1

        elif sens == 'bas_vers_haut':
            # Parcourir du bas vers le haut
            idx_front = None
            for y in range(hauteur - 1, -1, -1):
                if colonne_norm[y] < seuil_detection:
                    idx_front = y
                    break
            if idx_front is None:
                idx_front = 0
        else:
            raise ValueError("Le paramètre 'sens' doit être 'haut_vers_bas' ou 'bas_vers_haut'")

        # Debug: afficher la position détectée pour quelques colonnes
        if x % (largeur // 5) == 0:
            print(f"  Position front détectée: y={idx_front}, valeur={colonne_norm[idx_front]:.3f}")

        temps.append(x)
        hauteurs.append(idx_front)

    print(f"\nNombre de points extraits: {len(temps)}")
    print(f"Hauteurs - Min: {min(hauteurs)}, Max: {max(hauteurs)}")

    return np.array(temps), np.array(hauteurs)


def visualiser_resultat(chemin_image, temps, hauteurs):
    """Visualise l'image et la courbe extraite."""
    img = cv2.imread(chemin_image)

    fig, (courbe, courbe_image) = plt.subplots(2, 1, figsize=(10, 10))

    # Afficher la courbe seule
    courbe.plot(temps, hauteurs, 'b-o', linewidth=2, markersize=4)
    courbe.set_xlabel('Temps (pixels)')
    courbe.set_ylabel('Hauteur (pixels)')
    courbe.set_title('Courbe de décantation')
    courbe.grid(True)
    courbe.invert_yaxis()

    # Afficher l'image avec la courbe superposée
    courbe_image.imshow(cv2.cvtColor(img, cv2.COLOR_BGR2RGB))
    courbe_image.plot(temps, hauteurs, 'r-', linewidth=2, label='Front de décantation')
    courbe_image.set_xlabel('Temps (pixels)')
    courbe_image.set_ylabel('Hauteur (pixels)')
    courbe_image.set_title('Image avec courbe détectée')
    courbe_image.legend()

    plt.tight_layout()
    plt.show()


def convertir_en_cm(hauteurs_pixels, facteur_conversion):
    """
    Convertit les hauteurs de pixels en centimètres.

    Args:
        hauteurs_pixels: Array des hauteurs en pixels
        facteur_conversion: Facteur de conversion (cm/pixel)

    Returns:
        hauteurs_cm: Array des hauteurs en centimètres
    """
    return hauteurs_pixels * facteur_conversion


facteur_cm_par_pixel = 0.1  # Exemple: 1 pixel = 0.1 cm

# Exemple d'utilisation
if __name__ == "__main__":
    chemin = "01.jpg"

    # Extraire la courbe avec sens de parcours
    print("=== Extraction avec sens 'haut_vers_bas' ===")
    temps, hauteurs = extraire_courbe_decantation(
        chemin,
        seuil_detection=0.90,
        sens='haut_vers_bas'  # ou 'bas_vers_haut'
    )

    # Conversion en cm
    hauteurs_cm = convertir_en_cm(hauteurs, facteur_cm_par_pixel)

    # Visualiser
    visualiser_resultat(chemin, temps, hauteurs)

    # Sauvegarder les données
    np.savetxt('courbe_decantation.csv',
               np.column_stack([temps, hauteurs, hauteurs_cm]),
               delimiter=',',
               header='Temps(px),Hauteur(px),Hauteur(cm)',
               comments='')
