# VillaBookingMAUI

Мобилно приложение за управление на резервации на вили, разработено с .NET MAUI.  
Свързва се с **VillaBookingAPI** за синхронизация на данни между устройства.

## Архитектура

Проектът следва **MVVM** (Model-View-ViewModel) шаблон:

```
VillaBookingMAUI/
├── Models/              # Модели (Booking, ApiResponse)
├── Services/            # API комуникация и свързаност
│   ├── IBookingApiService.cs
│   ├── BookingApiService.cs
│   └── ConnectivityService.cs
├── ViewModels/          # Логика на страниците
│   ├── BaseViewModel.cs
│   ├── HomeViewModel.cs
│   ├── BookingsListViewModel.cs
│   ├── BookingDetailViewModel.cs
│   ├── BookingFormViewModel.cs
│   └── CalendarViewModel.cs
├── Views/               # XAML страници
│   ├── HomePage.xaml
│   ├── BookingsListPage.xaml
│   ├── BookingDetailPage.xaml
│   ├── BookingFormPage.xaml
│   └── CalendarPage.xaml
├── Converters/          # Value конвертори за binding
├── AppShell.xaml        # Shell навигация (Tab bar)
├── MauiProgram.cs       # Dependency Injection конфигурация
└── App.xaml             # Глобални ресурси и стилове
```

## Страници

| Страница       | Описание                                           |
|----------------|----------------------------------------------------|
| **Начало**     | Табло с обобщение, статистики и предстоящи резервации |
| **Резервации** | Списък с търсене, филтриране и swipe-to-delete     |
| **Календар**   | Месечна решетка с цветово кодиране на заети дни    |
| **Детайли**    | Пълна информация + споделяне, редактиране, изтриване |
| **Форма**      | Създаване/редактиране с валидация и проверка наличност |

## Използвани възможности на телефона

- **Мрежова свързаност** – Connectivity API за офлайн детекция
- **Хаптична обратна връзка** – HapticFeedback при успешни операции
- **Вибрация** – Vibration API при грешки във валидацията
- **Споделяне** – Share API за изпращане на информация за резервация
- **Информация за устройството** – DeviceInfo за автоматично попълване на "CreatedBy"

## Как да стартирате

### Предварителни изисквания

- .NET 8 SDK
- Visual Studio 2022 17.8+ с MAUI workload
- Android емулатор или физическо устройство
- Работещ VillaBookingAPI

### Стъпки

1. **Стартирайте API-то първо:**
   ```bash
   cd VillaBookingAPI
   dotnet run
   ```

2. **Конфигурирайте URL-а на API-то** в `MauiProgram.cs`:
   ```csharp
   // За Android емулатор:
   baseUrl = "https://10.0.2.2:7152";
   // За физическо устройство (сменете с вашия IP):
   baseUrl = "https://192.168.1.100:7152";
   ```

3. **Стартирайте MAUI приложението:**
   ```bash
   cd VillaBookingMAUI
   dotnet build -t:Run -f net8.0-android
   ```
   Или отворете `.csproj` във Visual Studio и натиснете F5.

### Важно за Android емулатор

Android емулаторът не може да достъпи `localhost` на хост машината.
Използвайте `10.0.2.2` вместо `localhost` (вече е конфигурирано в `MauiProgram.cs`).

За HTTP трафик (без SSL), добавете `android:usesCleartextTraffic="true"` в
`AndroidManifest.xml` (вече е добавено).

## NuGet пакети

- **CommunityToolkit.Mvvm** – ObservableObject, RelayCommand, ObservableProperty
- **CommunityToolkit.Maui** – Конвертори, анимации, popup-и
- **System.Text.Json** – JSON сериализация/десериализация

## Бележки за курсовия проект

Проектът покрива следните изисквания от заданието:
- Съвременен, удобен GUI на български език
- Използване на сензори/възможности на телефона (свързаност, вибрация, споделяне)
- ООП принципи и MVVM архитектурен шаблон
- Интерактивен интерфейс с pull-to-refresh, swipe-to-delete, навигация
- Валидация и обработка на грешки
- Dependency Injection
- Async/await навсякъде
