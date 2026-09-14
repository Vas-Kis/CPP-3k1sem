# CrossApp
 Наскрізний проєкт з крос-платформного програмування.
 Предметна область: Склад. Сутності: Product (товар), StockBatch (партія), Warehouse (склад), Movement (переміщення). 
 Призначення: облік залишків товарів по партіях.

 ## Запуск
 dotnet build
 dotnet run --project src/Cli
 ## Середовище
 .NET SDK 10.0, Windows 11 x64

 ## Додаткове завдання:
 У каталозі publish для RID linux-x64 78,77 МБ
 У каталозі publish для RID win-x64 76,62 МБ 

 ## Різниця self-contained vs framework-dependent:
 Framework-dependent публікація: лише код і залежності, без runtime.
 Self-contained публікація: каталог великий і прив’язаний до RID.


| RID | Режим | Розмір publish | Потрібен runtime |
| :--- | :---: | :---: | :---: |
| win-x64 | self-contained | 153,71 | ні |
| win-x64 | framework-dependent | 0,39 | так (.NET 10) |
| linux-x64 | self-contained | 157,58 | ні |