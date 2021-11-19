using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Media;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml.Schema;

namespace Pb_scientifique_Info
{
    public class MyImage
    {
        //attribues
        public string typeimage;
        public int taillefichier;
        public int tailleoffset;
        public int largeur;
        public int hauteur;
        public int nombredebits;
        public pixel[,] image;

        public pixel[,] Image { get { return image; } }
        public int Largeur { get { return largeur; } }
        public int Hauteur { get { return hauteur; } }
        /// <summary>
        /// Classe MyImage permettant de modifier les caractéristiques de l'image
        /// </summary>
        /// <param name="file"></param> ficher de l'Image choisi par l'utilisateur
        public MyImage(string file) 
        {
            byte[] myfile = File.ReadAllBytes(file); //remplissage de tous les byte du fichier dans un seul tableau
            typeimage = Char.ConvertFromUtf32(myfile[0]) + Char.ConvertFromUtf32(myfile[1]); // Conversion de bytes en char
            byte[] datataille = { myfile[2], myfile[3], myfile[4], myfile[5] }; // taille de l'image codé sur 4 octets
            taillefichier = Convertir_Endian_To_Int(datataille);
            byte[] dataTailleOffset = { myfile[10], myfile[11], myfile[12], myfile[13] }; // taille offset sur 4 octets
            tailleoffset = Convertir_Endian_To_Int(dataTailleOffset);
            byte[] dataLar = { myfile[18], myfile[19], myfile[20], myfile[21] }; // largeur sur 4 octets
            largeur = Convertir_Endian_To_Int(dataLar);
            byte[] dataHaut = { myfile[22], myfile[23], myfile[24], myfile[25] }; // hauteur sur 4 octets
            hauteur = Convertir_Endian_To_Int(dataHaut);
            byte[] dataBitCou = { myfile[28], myfile[29] }; 
            nombredebits = Convertir_Endian_To_Int(dataBitCou);
            image = new pixel[hauteur, largeur]; // création de la matrice de pixel
            int k = tailleoffset; 
            /// remplissage de la matrice de pixel avec ceux de l'image en paramètre
            for (int i = 0; i < hauteur; i++) 
            {
                for (int j = 0; j < largeur; j++)
                {


                    pixel p = new pixel(myfile[k], myfile[k + 1], myfile[k + 2]);
                    image[i, j] = p;
                    k = k + 3;
                }
            }

        }
        /// <summary>
        /// Classe Myimage permettant de créer une image a partir de sa taille (nécessaire pour certaines fonctions)
        /// </summary>
        /// <param name="hauteur"></param> hauteur de l'image
        /// <param name="largeur"></param> largeur de l'image
        public MyImage(int hauteur, int largeur) /// classe image avec pour paramètre la hauteur et la largeur 
        {
            typeimage = "BM";
            this.hauteur = hauteur;
            this.largeur = largeur;
            tailleoffset = 54;
            taillefichier = largeur * hauteur * 3 + tailleoffset;
            nombredebits = 24;
            image = new pixel[hauteur,largeur];
        }
        public void From_Image_To_File(string file) // Fonction permettant d'écrire le fichier et de l'enregistrer dans le répertoire avec en paramètre le nom du nouveau fichier
        {
            if (largeur % 4 == 0)
            {
                taillefichier = (hauteur * (largeur) * 3) + tailleoffset;

            }
            else
            {
                taillefichier = (hauteur * (largeur + 4 - largeur % 4) * 3) + tailleoffset;

            }
            //remplissage du header
            byte[] newfile = new byte[taillefichier];
            newfile[0] = 66; //B
            newfile[1] = 77; //M
            byte[] tailleFichiernew = Int_to_Endiant(taillefichier, 4);
            for (int i = 0; i < 4; i++)
            {
                newfile[i + 2] = tailleFichiernew[i];
            }
            for (int i = 6; i < 10; i++)
            {
                newfile[i] = 0;
            }
            byte[] tailleoffsetnew = Int_to_Endiant(tailleoffset, 4);
            for (int i = 0; i < 4; i++)
            {
                newfile[i + 10] = tailleoffsetnew[i];
            }
            newfile[14] = 40;
            newfile[15] = 0;
            newfile[16] = 0;
            newfile[17] = 0;
            byte[] largeurnew = Int_to_Endiant(largeur, 4);
            for (int i = 0; i < 4; i++)
            {
                newfile[i + 18] = largeurnew[i];
            }
            byte[] hauteurnew = Int_to_Endiant(hauteur, 4);
            for (int i = 0; i < 4; i++)
            {
                newfile[i + 22] = hauteurnew[i];
            }
            newfile[26] = 1;
            newfile[27] = 0;
            byte[] nombredebitsnew = Int_to_Endiant(nombredebits, 2);
            for (int i = 0; i < 2; i++)
            {
                newfile[i + 28] = nombredebitsnew[i];
            }
            for (int i = 30; i < 34; i++)
            {
                newfile[i] = 0;
            }
            byte[] tailleimage = Int_to_Endiant(hauteur * largeur * 3, 4);
            for (int i = 0; i < 4; i++)
            {
                newfile[i + 34] = tailleimage[i];
            }
            for (int i =38; i < 54; i++)
            {
                newfile[i] = 0;
            }
            int p = tailleoffset;
            //remplissage des pixels
            for (int i = 0; i < hauteur; i++)
            {
                for (int j = 0; j < largeur; j++)
                {
                    newfile[p] = (byte)(image[i, j].R);
                    p++;
                    newfile[p] = (byte)(image[i, j].G);
                    p++;
                    newfile[p] = (byte)(image[i, j].B);
                    p++;
                }
            }
            
            File.WriteAllBytes(file, newfile); // enregistrement du fichier
        }
        public pixel[,] copieimage(pixel[,] img) // fonction qui copie l'image mise en paramètre afin de pouvoir modifier l'autre tout en l'utilisant
        {
            pixel[,] copie = new pixel[hauteur, largeur];
            for (int i = 0; i < hauteur; i++)
            {
                for (int j = 0; j < largeur; j++)
                {
                    copie[i, j] = img[i, j];
                }
            }
            return copie;
        }
        public int Convertir_Endian_To_Int(byte[] tab) // fonction permettant la conversion d'un tableau de byte Endian en un nombre
        {
            int index = 0;
            if (tab.Length == 4)
            {
                return (tab[index + 3] << 24) | (tab[index + 2] << 16) | tab[index + 1] << 8 | tab[index];
            }
            else
            {
                return ((tab[index + 1] << 8) | tab[index]);
            }
        }
        public byte[] Int_to_Endiant(int val, int taille) // fonction de conversion d'un nombre en un endian de taille voulu
        {
            byte[] tab = new byte[taille];
            if (taille == 2)
            {
                tab[1] = (byte)((val >> 8) & 0xff);
                tab[0] = (byte)(val & 0xff);
            }
            if (taille == 4)
            {
                tab[3] = (byte)((val >> 24) & 0xff);
                tab[2] = (byte)((val >> 16) & 0xff);
                tab[1] = (byte)((val >> 8) & 0xff);
                tab[0] = (byte)(val & 0xff);
            }

            return tab;
        }
        public void Nuancedegris() // fonction permettant de modifier la photo en gris
        {
            for (int i = 0; i < hauteur; i++)
            {
                for (int j = 0; j < largeur; j++)
                {
                    image[i, j].nuancesGris();
                }
            }
        }
        public void Noiretblanc(int seuil) //fonction permettant de modifier la photo en noir et blanc
        {
            for (int i = 0; i < hauteur; i++)
            {
                for (int j = 0; j < largeur; j++)
                {
                    image[i, j].noirEtBlanc(seuil);
                }
            }
        }
        public void Miroir() //fonction permettant d'effectuer une modification miroir a l'image
        {
            pixel[,] premiere = copieimage(image);
            for (int i = 0; i < hauteur; i++)
            {
                for (int j = 0; j < largeur; j++)
                {
                    image[i, j] = premiere[i, largeur - 1 - j];
                }
            }
        }
        public void Rotation(int angle) // fonction de rotation de l'image
        {
            pixel[,] premiere = copieimage(image);
            if (angle == 0)
            {
                image = new pixel[hauteur, largeur];
                for (int i = 0; i < hauteur; i++)
                {
                    for (int j = 0; j < largeur; j++)
                    {
                        image[i, j] = premiere[i, j];
                    }
                }
            }
            if (angle == 270)
            {
                int h = hauteur;
                hauteur = largeur;
                largeur = h;
                image = new pixel[hauteur, largeur];
                for (int i = 0; i < hauteur; i++)
                {
                    for (int j = 0; j < largeur; j++)
                    {
                        image[i, j] = premiere[largeur - 1 - j, i];
                    }
                }
            }
            else if (angle == 180)
            {
                for (int i = 0; i < hauteur; i++)
                {
                    for (int j = 0; j < largeur; j++)
                    {
                        image[i, j] = premiere[hauteur - i - 1, largeur - j - 1];
                    }
                }
            }
            else if (angle == 90)
            {
                int h = hauteur;
                hauteur = largeur;
                largeur = h;
                image = new pixel[hauteur, largeur];
                for (int i = 0; i < hauteur; i++)
                {
                    for (int j = 0; j < largeur; j++)
                    {
                        image[i, j] = premiere[j, hauteur - 1 - i];
                    }
                }
            }
            else { Console.WriteLine("Impossible à éxécuter"); }
            // a refaire car valable seulement pour 4 angles 
        }
        public int multiple4reduire(int nb) // fonction pour transformer un nombre en multiple de 4
        {
            if (nb % 4 == 0)
            {
                return nb;
            }
            else
            {
                return nb - nb % 4;
            }
        }
        public void reduire(int echelle) // fonction qui permet de reduire l'image
        {
            pixel[,] imgdepart = copieimage(image);
            hauteur = multiple4reduire((hauteur / echelle));
            largeur = multiple4reduire((largeur / echelle));
            image = new pixel[hauteur, largeur];
            for (int i = 0; i < hauteur; i++)
            {
                for (int j = 0; j < largeur; j++)
                {
                    image[i, j] = new pixel(0, 0, 0);
                }
            }
            for (int i = 0; i < hauteur; i++)
            {
                for (int j = 0; j < largeur; j++)
                {
                    int sommeR = 0;
                    int sommeG = 0;
                    int sommeB = 0;
                    for (int k = 0; k < echelle; k++)
                    {
                        for (int l = 0; l < echelle; l++)
                        {
                            sommeR += imgdepart[i * echelle + k, j * echelle + l].R;
                            sommeG += imgdepart[i * echelle + k, j * echelle + l].G;
                            sommeB += imgdepart[i * echelle + k, j * echelle + l].B;
                        }
                    }
                    image[i, j].R = sommeR / (echelle * echelle);
                    image[i, j].G = sommeG / (echelle * echelle);
                    image[i, j].B = sommeB / (echelle * echelle);
                }
            }
        }
        public void agrandir(int echelle) // fonction qui permet d'agrandir l'image
        {
            pixel[,] imgdepart = copieimage(image);
            largeur = largeur * echelle;
            hauteur = hauteur * echelle;
            image = new pixel[hauteur, largeur];
            for (int i = 0; i < hauteur; i++)
            {
                for (int j = 0; j < largeur; j++)
                {
                    image[i, j] = new pixel(0, 0, 0);
                }
            }
            for (int i = 0; i < hauteur; i++)
            {
                for (int j = 0; j < largeur; j++)
                {
                    image[i, j].R = imgdepart[i / echelle, j / echelle].R;
                    image[i, j].G = imgdepart[i / echelle, j / echelle].G;
                    image[i, j].B = imgdepart[i / echelle, j / echelle].B;
                }
            }
        }
        public int outofrange(int nb, int longueur) // fonction qui permet de ne pas depasser l'index
        {
            if (nb < 0)
            {
                return 0;
            }
            if (nb >= longueur)
            {
                return longueur - 1;
            }
            return nb;
        }
        public int normalisation(int nb) /// evite de dépaser les max et les mins d'un pixel 
        {
            if (nb < 0)
            {
                nb = -nb;
            }
            if (nb > 255)
            {
                nb = 255;
            }
            return nb;
        }
        public void Convolution(int[,] matrice) // fonction de convolution avec pour paramètre une matrice de convolution 
        {
            pixel[,] newimg = copieimage(image);
            int diviseur = 0;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    diviseur += matrice[i, j];
                }
            }
            if (diviseur == 0)
            { diviseur = 1; }
            for (int i = 0; i < hauteur; i++)
            {
                for (int j = 0; j < largeur; j++)
                {

                    int sommeR = 0;
                    int sommeG = 0;
                    int sommeB = 0;
                    for (int h = -1; h < 2; h++)
                    {
                        for (int l = -1; l < 2; l++)
                        {
                            int x = outofrange(i + h, hauteur);
                            int y = outofrange(j + l, largeur);
                            sommeR += newimg[x, y].R * matrice[h + 1, l + 1];
                            sommeG += newimg[x, y].G * matrice[h + 1, l + 1];
                            sommeB += newimg[x, y].B * matrice[h + 1, l + 1];
                        }
                    }
                    image[i, j].R = normalisation(sommeR / diviseur);
                    image[i, j].G = normalisation(sommeG / diviseur);
                    image[i, j].B = normalisation(sommeB / diviseur);
                }
            }
        }
        public void mandelbrot(int itemax) // fonction déterminant la fractale de mandelbrot
        {
            image = new pixel[hauteur, largeur];
            for (int i = 0; i < hauteur; i++)
            {
                for (int j = 0; j < largeur; j++)
                {
                    image[i, j] = new pixel(0, 0, 0);
                }
            }

            for (int x = 0; x < hauteur; x++)
            {
                for (int y = 0; y < largeur; y++)
                {

                    double constréel = (x-hauteur/2) / (0.5*hauteur); // calcul constante  réel de départ
                    double constimg = (y-largeur/2) / (0.5*largeur); // calcul constante imaginaire de départ
                    double réel = 0 ;
                    double img = 0;
                    int n = 0; // nombre d'itérations
                    while (n < itemax)
                    {
                        double réelsup = réel * réel - img * img + constréel; // calcul du réel suivant
                        double imgsup = 2 * réel * img + constimg; // calcul de l'imaginaire du complexe suivant
                        réel = réelsup + constréel;
                        img = imgsup + constimg;
                        if (réel*réel +img* img >= 4) // vérification des conditions de fin
                        {
                            break;
                        }
                        n++;


                    }
                    int luminosité = 255- (n*255/itemax) ;
                    image[y,x].R = luminosité/2;
                    image[y, x].G =luminosité/8 ;
                    image[y, x].B = luminosité*4;
                }
            }
        }
        public void histogramme()  /// Détermination de l'histogramme
        {
            pixel[,] imgdepart = copieimage(image); 
            //separation en 3 pour les 3 couleurs
            int[] histo = new int[256]; 
            int[] histo2 = new int[ 256];
            int[] histo3 = new int[ 256];
            for (int i = 0; i < 256; i++) // initialisation a 0
            {
                histo[i] = 0;
                histo2[i] = 0;
                histo3[i] = 0;
            }
            /// remplissage des 3 tableaux
            for (int i = 0; i < hauteur; i++)
            {
                for (int j = 0; j < largeur; j++)
                {
                    int index = imgdepart[i, j].R; 
                    histo[index]++;
                    int index2 = imgdepart[i, j].G;
                    histo2[index2]++;
                    int index3 = imgdepart[i, j].B;
                    histo3[index3]++;
                }
            }
            /// changement de l'échelle rendre l'histogramme plus propre 
            for (int i = 0; i < 256; i++)
            {
                histo[i] = histo[i] / 10; 
                histo2[i] = histo2[i] / 10;
                histo3[i] = histo3[i] / 10;
            }
            int max = 0;
            int max2 = 0;
            int max3 = 0;
             //détermination de la taille  de chaque histogramme a partir de l'index qui contient le plus de pixel
            for (int i = 0; i < 256; i++)
            {
                if(histo[i]> max)
                {
                    max += histo[i];
                }
                if (histo2[i] > max2)
                {
                    max2 += histo2[i];
                }
                if (histo3[i] > max3)
                {
                    max3 += histo3[i];
                }
            }
            image = new pixel[max + max2 + max3 + 10, 256]; // création de l'image de l'histogramme en fonction de sa taille
            hauteur = max + max2 + max3 + 10;
            largeur = 256;
            // initialisation a 0
            for (int i = 0; i < hauteur; i++)
            {
                for (int j = 0; j < largeur; j++)
                {
                    image[i, j] = new pixel(0, 0, 0);
                }
            }
            //remplissage avec les valeurs 
            for (int j = 0; j < largeur; j++)
            {
                for (int i = 0; i < max; i++)
                {
                    if (i <=histo[j])
                    {
                        image[i, j].R = 0;
                        image[i, j].G = 0;
                        image[i, j].B = j;
                    }
                    else
                    {
                        image[i, j].R = 0;
                        image[i, j].G = 0;
                        image[i, j].B = 0;
                    }
                }
                for (int i = max; i < max+5; i++)
                {
                    image[i, j].R = 0;
                    image[i, j].G = 0;
                    image[i, j].B = 0;
                }
                for (int i = max +5; i < max+max2+5; i++)
                {
                    if (i <= histo2[j]+max+5)
                    {
                        image[i, j].R = 0;
                        image[i, j].G = j;
                        image[i, j].B = 0;
                    }
                    else
                    {
                        image[i, j].R = 0;
                        image[i, j].G = 0;
                        image[i, j].B = 0;
                    }
                }
                for (int i = max+max2+5; i < max+max2+10; i++)
                {
                    image[i, j].R = 0;
                    image[i, j].G = 0;
                    image[i, j].B = 0;
                }
                for (int i = max + max2 + 10; i < max + max2 + 10+max3; i++)
                {
                    if (i <= histo3[j] + max + 5+max2)
                    {
                        image[i, j].R =j;
                        image[i, j].G = 0;
                        image[i, j].B = 0;
                    }
                    else
                    {
                        image[i, j].R = 0;
                        image[i, j].G = 0;
                        image[i, j].B = 0;
                    }
                }
            }

        }
        public static string convDecBin(int dec, int taille) // fonction permettant de convertir des nombres decimaux en binaire
        {
            string res = "";
            int div;
            int i;
            do
            {

                div = 0;
                i = dec;

                while (i != 1 && i != 0)
                {
                    i -= 2;
                    div += 1;
                }

                res = Convert.ToString(dec - (2 * div)) + res;
                dec = div;

            }
            while (dec != 0);           
            while (res.Length < taille) // transformation du nombre decimal en une chaine binaire de la taille voulue
            {
                res = "0" + res;
            }            
            return res;
        }
        public static int convBinDec(string binar) // fonction de transformation d'un nombre  binaire en nombre decimal
        {
            int taille = binar.Length;
            int puissance = taille - 1;
            double soustotal;
            int total = 0;

            if (binar == "1")
            { total = 1; }
            else
            {
                if (binar == "0")
                { total = 0; }
                else
                {

                    for (int i = 0; i < taille; i++)
                    {
                        if ((puissance >= 0) && (binar[i] == '1'))
                        {

                            soustotal = Math.Pow(2, puissance);

                            puissance -= 1;
                            total += Convert.ToInt32(soustotal);

                        }
                        else
                        {
                            puissance -= 1;
                        }

                    }
                }

            }
            return total;
        }
        public void steganographie(MyImage img2)
        {
            pixel[,] imgdepart = copieimage(image);
            image = new pixel[hauteur, largeur];
            while (hauteur < img2.hauteur || largeur < img2.hauteur) // Afin de ne pas couper l'image on la reduit si celle ci est plus grande que l'image principale
            {
                img2.reduire(2);
            }
            for (int i = 0; i < hauteur; i++) // initialisation de l'image
            {
                for (int j = 0; j < largeur; j++)
                {
                    image[i, j] = new pixel(255, 255, 255);
                }
            }
            for (int i = 0; i < hauteur; i++)
            {
                for (int j = 0; j < largeur; j++)
                {
                    if (i < img2.hauteur && j < img2.largeur)
                    {
                        string rouge = convDecBin(imgdepart[i, j].R, 8);
                        string bleu = convDecBin(imgdepart[i, j].B, 8);
                        string vert = convDecBin(imgdepart[i, j].G, 8);
                        string rouge2 = convDecBin(img2.image[i, j].R, 8);
                        string bleu2 = convDecBin(img2.image[i, j].B, 8);
                        string vert2 = convDecBin(img2.image[i, j].G, 8);
                        // Les bits de poids forts l'image sont les bits de poids forts de l'image principale
                        // Les bits de poids faibles de l'image sont les bits de poids forts de l'image caché
                        string rougef = "" + rouge[0] + rouge[1] + rouge[2] + rouge[3] + rouge2[0] + rouge2[1] + rouge2[2] + rouge2[3]; 
                        string vertf = "" + vert[0] + vert[1] + vert[2] + vert[3] + vert2[0] + vert2[1] + vert2[2] + vert2[3];
                        string bleuf = "" + bleu[0] + bleu[1] + bleu[2] + bleu[3] + bleu2[0] + bleu2[1] + bleu2[2] + bleu2[3];

                        image[i, j].R = convBinDec(rougef);
                        image[i, j].G = convBinDec(vertf);
                        image[i, j].B = convBinDec(bleuf);
                    }
                    else
                    {
                        string rouge = convDecBin(imgdepart[i, j].R, 8);
                        string bleu = convDecBin(imgdepart[i, j].B, 8);
                        string vert = convDecBin(imgdepart[i, j].G, 8);
                        // Si l'image n'atteint pas ces indexs on la remplace par du blanc (question d'esthetisme)
                        string rouge2 = convDecBin(255, 8); 
                        string bleu2 = convDecBin(255, 8);
                        string vert2 = convDecBin(255, 8);
                        string rougef = "" + rouge[0] + rouge[1] + rouge[2] + rouge[3] + rouge2[0] + rouge2[1] + rouge2[2] + rouge2[3];
                        string vertf = "" + vert[0] + vert[1] + vert[2] + vert[3] + vert2[0] + vert2[1] + vert2[2] + vert2[3];
                        string bleuf = "" + bleu[0] + bleu[1] + bleu[2] + bleu[3] + bleu2[0] + bleu2[1] + bleu2[2] + bleu2[3];

                        image[i, j].R = convBinDec(rougef);
                        image[i, j].G = convBinDec(vertf);
                        image[i, j].B = convBinDec(bleuf);
                    }
                }
            }
        }
        public void decoderSteganographie() // decodage de l'image caché
        {
            pixel[,] imgdepart = copieimage(image);
            for (int i = 0; i < hauteur; i++)
            {
                for (int j = 0; j < largeur; j++)
                {
                    string rouge = convDecBin(imgdepart[i, j].R, 8);
                    string bleu = convDecBin(imgdepart[i, j].B, 8);
                    string vert = convDecBin(imgdepart[i, j].G, 8);
                    //les bits de poids forts de l'image caché sont les bits de poids faible de l'image de depart
                    string rougef = "" + rouge[4] + rouge[5] + rouge[6] + rouge[7] +"0000"; 
                    string vertf = "" + vert[4] + vert[5] + vert[6] + vert[7] + "0000";
                    string bleuf = "" + bleu[4] + bleu[5] + bleu[6] + bleu[7] + "0000";

                    image[i, j].R = convBinDec(rougef);
                    image[i, j].G = convBinDec(vertf);
                    image[i, j].B = convBinDec(bleuf);
                }
            }
        }   
    }
}
