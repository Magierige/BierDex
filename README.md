# BierDex
De volgende dreigingen zijn gemitigeerd of geëlimineerd door de genomen beveiligingsmaatregelen:

- Threads 222, 196 en 220 zijn geëlimineerd door het gebruik van HTTPS, waardoor communicatie tussen client en server versleuteld is en niet kan worden onderschept of gemanipuleerd.
- Threads 219, 212, 208 en 205 zijn gemitigeerd door het gebruik van sterke, veilige en geheime wachtwoorden voor externe systemen, zoals de database.
- Threads 218, 203 en 195 zijn gemitigeerd door alle logica en inputvalidatie (sanitization) server-side uit te voeren, in plaats van te vertrouwen op client-side verwerking.
- Threads 217, 211 en 206 zijn geëlimineerd door gebruik te maken van Entity Framework, waardoor SQL-injectie wordt voorkomen via parameterized queries.
- Thread 204 is geëlimineerd door het implementeren van CSRF-tokens in formulieren, waardoor ongeautoriseerde verzoeken worden tegengegaan.
