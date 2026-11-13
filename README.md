# Elkartea_TPV

## Proiektuaren deskribapena

Elkartea_TPV elkarte gastronomiko baten kudeaketa errazteko garatutako mahaigaineko aplikazioa da. Aplikazioaren bidez erabiltzaileak, biltegia, kontsumoak eta mahai-erreserbak kudeatu daitezke.

## Aurrebaldintzak

Aplikazioa martxan jartzeko, honako hauek behar dira:

- Windows sistema eragilea.
- Visual Studio instalatuta.
- Proiektuak eskatzen duen .NET SDK bertsioa.
- Git, proiektua GitHubetik deskargatzeko.

## Proiektua deskargatzea

1. Ireki proiektuaren GitHub biltegia.
2. Sakatu Code botoian.
3. Kopiatu HTTPS helbidea.
4. Ireki terminala eta exekutatu:

git clone GITHUB_BILTEGIAREN_HELBIDEA

5. Ireki deskargatutako proiektua Visual Studio erabiliz.

## Aplikazioa martxan jartzea

1. Ireki `Elkartea_TPV.sln` fitxategia Visual Studio erabiliz.
2. Itxaron beharrezko NuGet paketeak kargatu arte.
3. Ezarri `Elkartea_TPV` abiarazteko proiektu nagusi gisa.
4. Sakatu Start botoia edo `F5` tekla.
5. Aplikazioan saioa hasteko leihoa agertuko da.

## Lehenengo sarbidea

Aplikazioa lehen aldiz exekutatzean, datu-basea automatikoki sortzen da.

Hasierako administratzailearen datuak:

- Erabiltzaile-izena:admin
- Pasahitza: admin123

Erabiltzaile arruntaren probako datuak:

- Erabiltzaile-izena:proba
- Pasahitza: 123456

- Erabiltzaile-izena: haizea
- Pasahitza: 123456

## Ohar gehigarriak

- Produktuen irudiak `Irudiak` karpetan daude. Karpeta hori proiektuaren fitxategiekin batera mantendu behar da.
- Aplikazioak SQLite erabiltzen du, eta `elkartea_tpv.db` datu-basea automatikoki sortzen da lehenengo aldiz abiarazterakoan.