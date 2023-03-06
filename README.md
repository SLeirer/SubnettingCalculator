MiniMax Algorhytmus anhand TicTacToe Spiel In C++
=================
# Inhalt
[Infos](#infos)  
[Anleitung](#anleitung)  
[Nachträgliches](#nachträgliches)  
[Genutzte Software und resourcen](#zur-projekterstellung-genutze-software-und-resourcen)

## Infos
Projekt Aus dem Ende des 2ten Jahres / Anfang 3ten jahres meiner Ausbildung.
Es handelt sich Um einene SubnetzRechner.<br>
Es lässt sich eine NetID und subnetz eingeben woraus ein netz erstellt wird welches sich variable teilen und zusammenfügen lässt.
Für das Projekt wurde eine vollständige Projektdokumentation erstellt die ich zu einem späterne Zeitpunkt hinzufügen werde.

## Anleitung

* Eingabe einer netID und einer subnetz slash-notation zur erstellung eines netzes.
* Netze lasse sich über dynamisch generierte buttons teilen bis das kleinste Netz erreicht wird.
* Netze lassen sich über dynamisch generierte buttons wieder zusammenfügen.

### Nachträgliches
Im nachhinein hätte ich das projekt mit IP's anstatt NetID's zur Eingabe erstellt.
Die Eingabe und Errechnung der Subnetzte über die NetID führt zu einem folgenden Problem wobei die Teil-netze sich nur schwer wieder zusammenzufügen lassen.
Ich habe mich am ende für eine Chronik entschieden wo jedes Teil-netzobjekt eine Liste enthält von Netzen von denen es abgeleitet wurde,
und sich Darüber wieder mit zusammenhängenden Teil-netzen zusammenfügen lässt.
Wenn ich es von Anfang an mit IP eingabe erstellt hätte, könnte dies auf einfachen weg rechnerisch zurückverfolgt werden.

## Zur projekterstellung genutze software und resourcen

* Visual Studio community edition 2020.
* .Net Razor-pages.
* Subentzberechnungen wurden selber erstellt, aus wissen vermittelt durch die Ausbildung.
