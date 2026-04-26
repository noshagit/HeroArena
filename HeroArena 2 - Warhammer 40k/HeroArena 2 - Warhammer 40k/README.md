# HeroArena

[Lien vers le répertoire git](https://github.com/noshagit/HeroArena.git)

Commande pour cloner le projet :

```bash
git clone https://github.com/noshagit/HeroArena.git
```

## Sommaire

- [HeroArena](#heroarena)
  - [Sommaire](#sommaire)
  - [Prérequis](#prérequis)
  - [Mise en place de la base de données](#mise-en-place-de-la-base-de-données)
    - [Installer SQL Server Express 2022](#installer-sql-server-express-2022)
    - [Installer SSMS 2026](#installer-ssms-2026)
    - [Créer la base ExerciceHero](#créer-la-base-exercicehero)
  - [Configurer la connexion dans le jeu](#configurer-la-connexion-dans-le-jeu)
  - [Utiliser la fenêtre Paramètres BD](#utiliser-la-fenêtre-paramètres-bd)
  - [Script SQL à exécuter si la base est vide](#script-sql-à-exécuter-si-la-base-est-vide)
    - [1. Création des tables](#1-création-des-tables)
    - [2. Insertion des données (sorts, héros, relations)](#2-insertion-des-données-sorts-héros-relations)
  - [Lancer le jeu](#lancer-le-jeu)

## Prérequis

Pour exécuter le projet, il faut Visual Studio 2022 avec le workload .NET Desktop Development, le SDK .NET 8, SQL Server Express 2022 et SQL Server Management Studio (SSMS) 2026.

## Mise en place de la base de données

### Installer SQL Server Express 2022

Installe SQL Server Express 2022 puis note le nom de l’instance créée. Dans la plupart des cas, l’instance s’appelle SQLEXPRESS. Selon l’installation, tu te connecteras donc à quelque chose comme localhost\SQLEXPRESS ou .\SQLEXPRESS.

### Installer SSMS 2026

Installe SSMS 2026, puis connecte-toi à ton serveur SQL (par exemple localhost\SQLEXPRESS).

### Créer la base ExerciceHero

Dans SSMS, crée une base de données nommée exactement ExerciceHero.

## Configurer la connexion dans le jeu

La chaîne de connexion par défaut se trouve dans Models/AppSettings.cs.

Il faut adapter le nom du serveur à ton installation. Dans ce fichier, modifie la propriété ConnectionString (chez toi, elle est vers la ligne 12) en remplaçant uniquement la partie Server=... par le nom de ton serveur.

Exemple typique pour SQL Express local

```cs
Server=localhost\SQLEXPRESS;Database=ExerciceHero;Trusted_Connection=True;TrustServerCertificate=True;
```

Exemple de valeur déjà présente dans le projet

```cs
Server=NOSHA\SQLEXPRESS;Database=ExerciceHero;Trusted_Connection=True;TrustServerCertificate=True;
```

## Utiliser la fenêtre Paramètres BD

Au lancement, l’écran de connexion propose un bouton Paramètres BD.

Dans cette fenêtre, tu peux coller ta chaîne de connexion, tester la connexion, puis lancer l’initialisation des données si le bouton est disponible.

Si l’initialisation ne fonctionne pas ou si la base est vide, tu peux créer la structure et insérer les données manuellement avec le script ci-dessous.

## Script SQL à exécuter si la base est vide

Copie le script dans une nouvelle requête SSMS en étant bien placé sur la base ExerciceHero, puis exécute-le.

### 1. Création des tables

```sql
CREATE TABLE Login (
    ID INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL
);

CREATE TABLE Player (
    ID INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(50) NOT NULL,
    LoginID INT,
    FOREIGN KEY (LoginID) REFERENCES Login(ID)
);

CREATE TABLE Hero (
    ID INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(50) NOT NULL,
    Health INT NOT NULL,
    ImageURL NVARCHAR(255) NULL
);

CREATE TABLE Spell (
    ID INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(50) NOT NULL,
    Damage INT NOT NULL,
    Description NVARCHAR(MAX)
);

CREATE TABLE PlayerHero (
    PlayerID INT NOT NULL,
    HeroID INT NOT NULL,
    PRIMARY KEY (PlayerID, HeroID),
    FOREIGN KEY (PlayerID) REFERENCES Player(ID),
    FOREIGN KEY (HeroID) REFERENCES Hero(ID)
);

CREATE TABLE HeroSpell (
    HeroID INT NOT NULL,
    SpellID INT NOT NULL,
    PRIMARY KEY (HeroID, SpellID),
    FOREIGN KEY (HeroID) REFERENCES Hero(ID),
    FOREIGN KEY (SpellID) REFERENCES Spell(ID)
);
```

### 2. Insertion des données (sorts, héros, relations)

```sql
INSERT INTO Spell (Name, Damage, Description) VALUES
('Smite', 50, 'Puissance psychique brute projetee contre l ennemi'),
('Vortex of Doom', 80, 'Tourbillon du Warp devastateur'),
('Psychic Shriek', 35, 'Hurlement psychique qui dechire l esprit'),
('Gate of Infinity', 20, 'Teleportation instantanee a travers le Warp'),
('Bolter Salvo', 45, 'Rafale de bolts explosifs'),
('Hammer of Sigismund', 70, 'Coup de marteau de guerre beant'),
('Shield of Faith', 15, 'Bouclier de foi repoussant les assauts'),
('Righteous Zeal', 40, 'Charge fougueuse guidee par l Empereur'),
('Waaagh Energy', 55, 'Energie brute de la WAAAGH des Orks'),
('Choppa Frenzy', 65, 'Frenetique assaut de lames rouillees'),
('Gork Smash', 75, 'Poing de Gork ecrasant l ennemi'),
('Squig Bomb', 30, 'Squig explosif lance sur l ennemi'),
('Plague Wind', 40, 'Vent pestilentiel de Nurgle'),
('Blades of Putrefaction', 60, 'Lames putrefiees infligeant des blessures mortelles'),
('Death Guard March', 25, 'Avancee inexorable des guerriers de la mort'),
('Blight Grenade', 45, 'Grenade biologique de Nurgle');
GO

INSERT INTO Hero (Name, Health, ImageURL) VALUES
('Mephiston Libraire Ultramarines', 900, NULL),
('Celestine la Vivante Sainte', 1100, NULL),
('Ghazghkull Mag Uruk Thraka', 1300, NULL),
('Mortarion Primarque de la Death Guard', 1500, NULL);
GO

INSERT INTO HeroSpell (HeroID, SpellID) VALUES
(5,1),(5,2),(5,3),(5,4),
(6,5),(6,6),(6,7),(6,8),
(7,9),(7,10),(7,11),(7,12),
(8,13),(8,14),(8,15),(8,16);
GO
```

Remarque importante sur les IDs

Le bloc HeroSpell ci-dessus suppose que les IDs des héros commencent à 5. C’est le cas si ta base contient déjà des entrées (comme dans le contexte du projet).

Si tu viens de tout créer dans une base vide, les héros que tu insères auront plutôt des IDs 1 à 4. Dans ce cas, il faut simplement modifier les HeroID du bloc HeroSpell pour qu’ils correspondent aux IDs réels.

Pour vérifier rapidement les IDs

```sql
SELECT ID, Name FROM Hero ORDER BY ID;
SELECT ID, Name FROM Spell ORDER BY ID;
```

## Lancer le jeu

Ouvre le projet dans Visual Studio 2022 en demandant à ouvrir un projet et lance le jeu à partir du petit bouton play.

En cas de souci de connexion, vérifie le nom du serveur dans la chaîne ConnectionString, assure-toi que la base ExerciceHero existe bien, puis reteste depuis la fenêtre Paramètres BD.