using System;
using System.Collections.Generic;
using System.Text;
using Core.Models;

namespace Data.SampleData
{
    public class VacancyGenerator
    {
        private readonly Random _rng = new(42);

        // Каждый профиль — это связка: название, теги, стек, описание
        private readonly List<VacancyProfile> _profiles;

        private readonly string[] _companies =
        {
            "Яндекс", "VK", "Сбер", "Тинькофф", "Kaspersky Lab",
            "JetBrains", "Ozon", "Wildberries", "Авито", "HeadHunter",
            "МТС", "Мегафон", "Ростелеком", "Positive Technologies",
            "DataArt", "EPAM", "Luxoft", "Газпромнефть Цифровые решения",
            "Альфа-Банк", "Райффайзен Банк", "СКБ Контур", "1С",
            "Lamoda", "Delivery Club", "Циан"
        };

        private readonly string[] _locations =
        {
            "Москва", "Санкт-Петербург", "Новосибирск", "Екатеринбург",
            "Казань", "Нижний Новгород", "Краснодар", "Воронеж",
            "Удалённо", "Гибрид Москва", "Гибрид Санкт-Петербург"
        };

        private readonly string[] _employments =
        {
            "Полная занятость", "Частичная занятость", "Удалённая работа",
            "Проектная работа", "Стажировка"
        };

        private readonly string[] _levels = { "Junior", "Middle", "Senior", "Lead", "Principal" };

        public VacancyGenerator()
        {
            _profiles = BuildProfiles();
        }

        private List<VacancyProfile> BuildProfiles()
        {
            return new List<VacancyProfile>
            {
                // ── Python ──
                new VacancyProfile
                {
                    Titles = new[] { "Python разработчик", "Python Backend разработчик", "Django разработчик", "FastAPI разработчик" },
                    Tags = new[] { "python", "django", "fastapi", "flask", "postgresql", "redis", "docker", "celery" },
                    Stack = new[] { "Python", "Django", "FastAPI", "Flask", "SQLAlchemy", "Celery", "Redis", "PostgreSQL", "Docker", "REST API", "pytest" },
                    DescriptionParts = new[]
                    {
                        "Разрабатываем высоконагруженный backend на Python. ",
                        "Проектирование и реализация REST API, работа с базами данных PostgreSQL и Redis. ",
                        "Асинхронная обработка задач через Celery, деплой в Docker-контейнерах. ",
                        "Участие в код-ревью, написание unit и integration тестов. "
                    }
                },

                // ── Java ──
                new VacancyProfile
                {
                    Titles = new[] { "Java Developer", "Java разработчик", "Java Backend Developer", "Spring разработчик" },
                    Tags = new[] { "java", "spring", "spring boot", "microservices", "kafka", "postgresql", "docker", "kubernetes" },
                    Stack = new[] { "Java", "Spring Boot", "Spring Cloud", "Hibernate", "Maven", "Gradle", "Kafka", "RabbitMQ", "PostgreSQL", "MongoDB", "Docker", "Kubernetes" },
                    DescriptionParts = new[]
                    {
                        "Разработка микросервисной архитектуры на Java и Spring Boot. ",
                        "Проектирование API, интеграция с внешними сервисами через Kafka и RabbitMQ. ",
                        "Работа с реляционными и NoSQL базами данных. ",
                        "CI/CD пайплайны, контейнеризация, мониторинг в Kubernetes. "
                    }
                },

                // ── C# / .NET ──
                new VacancyProfile
                {
                    Titles = new[] { "C# разработчик", ".NET разработчик", "ASP.NET разработчик", "C# .NET Backend Developer" },
                    Tags = new[] { "c#", ".net", "asp.net", "entity framework", "sql server", "azure", "wpf", "blazor" },
                    Stack = new[] { "C#", ".NET 8", "ASP.NET Core", "Entity Framework Core", "MSSQL", "Azure", "SignalR", "Blazor", "gRPC", "Docker", "WPF" },
                    DescriptionParts = new[]
                    {
                        "Разработка корпоративных приложений на платформе .NET. ",
                        "Проектирование и реализация REST API на ASP.NET Core, работа с Entity Framework. ",
                        "Развёртывание в Azure, работа с очередями сообщений и кэшированием. ",
                        "Участие в архитектурных решениях, менторство коллег. "
                    }
                },

                // ── JavaScript / Frontend ──
                new VacancyProfile
                {
                    Titles = new[] { "Frontend разработчик", "React разработчик", "Vue.js разработчик", "JavaScript разработчик", "Frontend Developer" },
                    Tags = new[] { "javascript", "typescript", "react", "vue.js", "html", "css", "webpack", "frontend" },
                    Stack = new[] { "JavaScript", "TypeScript", "React", "Vue.js", "Next.js", "Nuxt.js", "Redux", "Vuex", "Webpack", "Vite", "HTML5", "CSS3", "SASS", "REST API", "GraphQL" },
                    DescriptionParts = new[]
                    {
                        "Разработка пользовательских интерфейсов на React и TypeScript. ",
                        "Создание переиспользуемых UI-компонентов, работа с состоянием приложения. ",
                        "Взаимодействие с backend через REST API и GraphQL. ",
                        "Оптимизация производительности, кроссбраузерная совместимость, адаптивная вёрстка. "
                    }
                },

                // ── JavaScript / Node.js Backend ──
                new VacancyProfile
                {
                    Titles = new[] { "Node.js разработчик", "Backend JavaScript Developer", "Fullstack JavaScript разработчик" },
                    Tags = new[] { "javascript", "typescript", "node.js", "express", "nestjs", "mongodb", "postgresql", "docker" },
                    Stack = new[] { "JavaScript", "TypeScript", "Node.js", "Express", "NestJS", "MongoDB", "PostgreSQL", "Redis", "Docker", "GraphQL", "Socket.IO" },
                    DescriptionParts = new[]
                    {
                        "Разработка серверной части приложений на Node.js и TypeScript. ",
                        "Проектирование RESTful и GraphQL API с использованием NestJS. ",
                        "Работа с MongoDB и PostgreSQL, кэширование в Redis. ",
                        "Написание тестов, участие в код-ревью, деплой через Docker. "
                    }
                },

                // ── Fullstack ──
                new VacancyProfile
                {
                    Titles = new[] { "Fullstack разработчик", "Fullstack Developer", "Full Stack Web Developer" },
                    Tags = new[] { "javascript", "typescript", "react", "node.js", "postgresql", "docker", "fullstack" },
                    Stack = new[] { "React", "TypeScript", "Node.js", "Express", "PostgreSQL", "MongoDB", "Docker", "REST API", "Git", "CI/CD" },
                    DescriptionParts = new[]
                    {
                        "Разработка web-приложений полного цикла — от интерфейса до серверной логики. ",
                        "Frontend на React с TypeScript, backend на Node.js. ",
                        "Проектирование баз данных, настройка CI/CD пайплайнов. ",
                        "Работа в agile-команде, участие в планировании спринтов. "
                    }
                },

                // ── Go ──
                new VacancyProfile
                {
                    Titles = new[] { "Go разработчик", "Golang Developer", "Backend Go разработчик" },
                    Tags = new[] { "go", "golang", "grpc", "postgresql", "redis", "docker", "kubernetes", "microservices" },
                    Stack = new[] { "Go", "gRPC", "Gin", "PostgreSQL", "Redis", "Docker", "Kubernetes", "Prometheus", "Grafana", "Kafka" },
                    DescriptionParts = new[]
                    {
                        "Разработка высоконагруженных микросервисов на Go. ",
                        "Проектирование API с использованием gRPC и REST. ",
                        "Оптимизация производительности, профилирование, бенчмарки. ",
                        "Деплой и оркестрация в Kubernetes, мониторинг через Prometheus и Grafana. "
                    }
                },

                // ── DevOps ──
                new VacancyProfile
                {
                    Titles = new[] { "DevOps инженер", "SRE инженер", "DevOps Engineer", "Infrastructure Engineer" },
                    Tags = new[] { "devops", "docker", "kubernetes", "terraform", "ansible", "ci/cd", "linux", "aws" },
                    Stack = new[] { "Docker", "Kubernetes", "Terraform", "Ansible", "Jenkins", "GitLab CI", "GitHub Actions", "AWS", "Linux", "Prometheus", "Grafana", "ELK Stack", "Bash" },
                    DescriptionParts = new[]
                    {
                        "Построение и поддержка CI/CD пайплайнов для продуктовых команд. ",
                        "Управление инфраструктурой через Infrastructure as Code (Terraform, Ansible). ",
                        "Оркестрация контейнеров в Kubernetes, мониторинг и алертинг. ",
                        "Обеспечение надёжности и отказоустойчивости production-окружения. "
                    }
                },

                // ── Data Science / ML ──
                new VacancyProfile
                {
                    Titles = new[] { "Data Scientist", "ML Engineer", "Аналитик данных", "Machine Learning инженер" },
                    Tags = new[] { "python", "ml", "machine learning", "tensorflow", "pytorch", "data science", "sql", "pandas" },
                    Stack = new[] { "Python", "pandas", "numpy", "scikit-learn", "TensorFlow", "PyTorch", "SQL", "Spark", "Airflow", "Jupyter", "matplotlib" },
                    DescriptionParts = new[]
                    {
                        "Разработка и внедрение моделей машинного обучения в продакшн. ",
                        "Анализ данных, построение пайплайнов обработки данных. ",
                        "Исследование новых подходов, A/B тестирование моделей. ",
                        "Работа с большими данными через Spark и Airflow. "
                    }
                },

                // ── QA ──
                new VacancyProfile
                {
                    Titles = new[] { "QA инженер", "Тестировщик", "Автоматизатор тестирования", "QA Automation Engineer" },
                    Tags = new[] { "qa", "тестирование", "selenium", "автотесты", "pytest", "api тестирование", "ci/cd" },
                    Stack = new[] { "Selenium", "Pytest", "JUnit", "Postman", "JMeter", "Allure", "CI/CD", "API тестирование", "нагрузочное тестирование", "Git" },
                    DescriptionParts = new[]
                    {
                        "Проектирование и автоматизация тестов для web-приложений. ",
                        "Тестирование REST API через Postman и автотесты. ",
                        "Нагрузочное тестирование, анализ результатов, поиск узких мест. ",
                        "Интеграция тестов в CI/CD пайплайн, формирование отчётов в Allure. "
                    }
                },

                // ── iOS ──
                new VacancyProfile
                {
                    Titles = new[] { "iOS разработчик", "iOS Developer", "Swift разработчик" },
                    Tags = new[] { "ios", "swift", "xcode", "swiftui", "uikit", "core data", "mobile" },
                    Stack = new[] { "Swift", "SwiftUI", "UIKit", "Xcode", "Core Data", "Combine", "REST API", "CocoaPods", "SPM", "MVVM" },
                    DescriptionParts = new[]
                    {
                        "Разработка мобильных приложений для iOS на Swift. ",
                        "Создание UI с помощью SwiftUI и UIKit, работа с Core Data. ",
                        "Интеграция с REST API, push-уведомления, аналитика. ",
                        "Публикация в App Store, работа с code review и CI/CD. "
                    }
                },

                // ── Android ──
                new VacancyProfile
                {
                    Titles = new[] { "Android разработчик", "Android Developer", "Kotlin разработчик" },
                    Tags = new[] { "android", "kotlin", "jetpack compose", "firebase", "mobile", "mvvm" },
                    Stack = new[] { "Kotlin", "Jetpack Compose", "Android SDK", "Room", "Retrofit", "Coroutines", "Firebase", "MVVM", "Clean Architecture", "Gradle" },
                    DescriptionParts = new[]
                    {
                        "Разработка Android-приложений на Kotlin с использованием Jetpack Compose. ",
                        "Архитектура Clean Architecture + MVVM, работа с Room и Retrofit. ",
                        "Интеграция с Firebase (push, analytics, crashlytics). ",
                        "Написание unit-тестов, участие в код-ревью. "
                    }
                },

                // ── PHP ──
                new VacancyProfile
                {
                    Titles = new[] { "PHP разработчик", "PHP Laravel разработчик", "Backend PHP Developer" },
                    Tags = new[] { "php", "laravel", "symfony", "mysql", "redis", "docker", "rest api" },
                    Stack = new[] { "PHP 8", "Laravel", "Symfony", "MySQL", "Redis", "Docker", "REST API", "Composer", "PHPUnit", "Nginx" },
                    DescriptionParts = new[]
                    {
                        "Разработка web-приложений на PHP и Laravel. ",
                        "Проектирование базы данных MySQL, оптимизация запросов. ",
                        "Создание REST API, интеграция с внешними сервисами. ",
                        "Написание тестов на PHPUnit, деплой через Docker. "
                    }
                },

                // ── Flutter ──
                new VacancyProfile
                {
                    Titles = new[] { "Flutter разработчик", "Flutter Developer", "Мобильный разработчик Flutter" },
                    Tags = new[] { "flutter", "dart", "mobile", "ios", "android", "firebase", "rest api" },
                    Stack = new[] { "Flutter", "Dart", "Firebase", "REST API", "BLoC", "Provider", "Riverpod", "SQLite", "Git" },
                    DescriptionParts = new[]
                    {
                        "Разработка кроссплатформенных мобильных приложений на Flutter. ",
                        "Архитектура BLoC/Provider, работа с REST API. ",
                        "Публикация в App Store и Google Play. ",
                        "Оптимизация производительности, анимации, адаптивный UI. "
                    }
                },

                // ── Rust ──
                new VacancyProfile
                {
                    Titles = new[] { "Rust Developer", "Rust разработчик", "Systems Programmer Rust" },
                    Tags = new[] { "rust", "systems programming", "performance", "linux", "concurrency" },
                    Stack = new[] { "Rust", "Tokio", "Actix", "PostgreSQL", "Linux", "Docker", "gRPC", "WebAssembly" },
                    DescriptionParts = new[]
                    {
                        "Разработка высокопроизводительных системных компонентов на Rust. ",
                        "Работа с многопоточностью и асинхронным программированием (Tokio). ",
                        "Оптимизация по памяти и скорости, zero-cost abstractions. ",
                        "Интеграция с существующими сервисами через gRPC и REST. "
                    }
                },

                // ── Системный администратор ──
                new VacancyProfile
                {
                    Titles = new[] { "Системный администратор", "Linux администратор", "Системный инженер" },
                    Tags = new[] { "linux", "администрирование", "nginx", "bash", "мониторинг", "сети" },
                    Stack = new[] { "Linux", "Nginx", "Apache", "Bash", "Python", "Zabbix", "Prometheus", "Grafana", "DNS", "TCP/IP", "iptables" },
                    DescriptionParts = new[]
                    {
                        "Администрирование Linux-серверов, настройка и поддержка инфраструктуры. ",
                        "Мониторинг через Zabbix и Prometheus, настройка алертинга. ",
                        "Управление сетевым оборудованием, настройка firewall и DNS. ",
                        "Автоматизация рутинных задач через Bash и Python скрипты. "
                    }
                },

                // ── Product Manager ──
                new VacancyProfile
                {
                    Titles = new[] { "Product Manager", "Продуктовый менеджер", "Product Owner" },
                    Tags = new[] { "product management", "agile", "scrum", "аналитика", "roadmap", "jira" },
                    Stack = new[] { "Jira", "Confluence", "Figma", "SQL", "Google Analytics", "Amplitude", "A/B тестирование", "Scrum", "Kanban" },
                    DescriptionParts = new[]
                    {
                        "Управление продуктовым бэклогом, приоритизация задач. ",
                        "Проведение исследований пользователей, анализ метрик продукта. ",
                        "Формирование roadmap, взаимодействие с командой разработки и дизайна. ",
                        "A/B тестирование гипотез, работа с аналитическими инструментами. "
                    }
                },

                // ── Архитектор ──
                new VacancyProfile
                {
                    Titles = new[] { "Архитектор решений", "Solution Architect", "Технический архитектор", "IT Architect" },
                    Tags = new[] { "архитектура", "microservices", "cloud", "highload", "проектирование", "aws", "azure" },
                    Stack = new[] { "Microservices", "AWS", "Azure", "Kubernetes", "Kafka", "gRPC", "REST", "DDD", "Event Sourcing", "CQRS", "PostgreSQL", "Redis" },
                    DescriptionParts = new[]
                    {
                        "Проектирование архитектуры высоконагруженных распределённых систем. ",
                        "Выбор технологического стека, проведение архитектурных ревью. ",
                        "Внедрение паттернов DDD, Event Sourcing, CQRS. ",
                        "Менторство команд разработки, техническое лидерство. "
                    }
                }
            };
        }

        public List<Vacancy> Generate(int count)
        {
            var vacancies = new List<Vacancy>(count);

            for (int i = 0; i < count; i++)
            {
                var profile = _profiles[_rng.Next(_profiles.Count)];
                var level = _levels[_rng.Next(_levels.Length)];
                var title = profile.Titles[_rng.Next(profile.Titles.Length)];

                // Добавляем уровень в название с вероятностью 60%
                if (_rng.NextDouble() < 0.6)
                {
                    title = $"{level} {title}";
                }

                var company = _companies[_rng.Next(_companies.Length)];
                var location = _locations[_rng.Next(_locations.Length)];
                var employment = _employments[_rng.Next(_employments.Length)];

                // Зарплатная вилка
                decimal? salaryFrom = null, salaryTo = null;
                if (_rng.NextDouble() < 0.7) // 70% вакансий с зарплатой
                {
                    int baseMultiplier = level switch
                    {
                        "Junior" => _rng.Next(40, 80),
                        "Middle" => _rng.Next(100, 200),
                        "Senior" => _rng.Next(200, 350),
                        "Lead" => _rng.Next(250, 450),
                        "Principal" => _rng.Next(350, 600),
                        _ => _rng.Next(80, 200)
                    };

                    salaryFrom = baseMultiplier * 1000m;
                    salaryTo = (baseMultiplier + _rng.Next(30, 100)) * 1000m;
                }

                // Теги — берём случайную подвыборку из профиля
                var tags = profile.Tags
                    .OrderBy(_ => _rng.Next())
                    .Take(_rng.Next(3, Math.Min(6, profile.Tags.Length + 1)))
                    .ToList();

                // Описание
                var description = GenerateDescription(profile, company, level);

                vacancies.Add(new Vacancy
                {
                    DocId = i + 1,
                    Title = title,
                    Company = company,
                    Location = location,
                    Employment = employment,
                    SalaryFrom = salaryFrom,
                    SalaryTo = salaryTo,
                    Tags = tags,
                    DatePosted = DateTime.Today.AddDays(-_rng.Next(0, 90)),
                    Description = description
                });
            }

            return vacancies;
        }

        private string GenerateDescription(VacancyProfile profile, string company, string level)
        {
            var sb = new System.Text.StringBuilder();

            // Вступление
            string[] intros =
            {
                $"Компания {company} ищет талантливого специалиста в команду разработки. ",
                $"Приглашаем {level}-специалиста в {company}. ",
                $"{company} открывает позицию в продуктовую команду. ",
                $"Мы — {company}, и мы ищем профессионала, готового к интересным задачам. "
            };
            sb.Append(intros[_rng.Next(intros.Length)]);

            // Основные описания из профиля
            foreach (var part in profile.DescriptionParts)
            {
                if (_rng.NextDouble() < 0.8)
                    sb.Append(part);
            }

            // Стек
            var stackItems = profile.Stack
                .OrderBy(_ => _rng.Next())
                .Take(_rng.Next(4, Math.Min(8, profile.Stack.Length + 1)));
            sb.Append($"Технологический стек: {string.Join(", ", stackItems)}. ");

            // Требования по уровню
            switch (level)
            {
                case "Junior":
                    sb.Append("Опыт работы от 0 до 1 года, готовность учиться и развиваться. ");
                    sb.Append("Менторство от senior-разработчиков, обучение за счёт компании. ");
                    break;
                case "Middle":
                    sb.Append("Опыт коммерческой разработки от 2 лет. ");
                    sb.Append("Умение самостоятельно декомпозировать и решать задачи. ");
                    break;
                case "Senior":
                    sb.Append("Опыт коммерческой разработки от 5 лет. ");
                    sb.Append("Навыки проектирования архитектуры, менторство junior и middle коллег. ");
                    break;
                case "Lead":
                    sb.Append("Опыт руководства командой от 3 человек. ");
                    sb.Append("Проведение собеседований, планирование спринтов, архитектурные решения. ");
                    break;
                case "Principal":
                    sb.Append("Экспертный уровень, влияние на техническую стратегию компании. ");
                    sb.Append("Кросс-командное взаимодействие, определение стандартов разработки. ");
                    break;
            }

            // Условия
            string[] conditions =
            {
                "Конкурентная заработная плата, ДМС, гибкий график. ",
                "Белая зарплата, бонусы по результатам, оплата конференций. ",
                "Удалённая работа, гибкое начало дня, бюджет на обучение. ",
                "Современный офис, бесплатные обеды, корпоративные мероприятия. "
            };
            sb.Append(conditions[_rng.Next(conditions.Length)]);

            return sb.ToString();
        }

        private class VacancyProfile
        {
            public string[] Titles { get; set; } = Array.Empty<string>();
            public string[] Tags { get; set; } = Array.Empty<string>();
            public string[] Stack { get; set; } = Array.Empty<string>();
            public string[] DescriptionParts { get; set; } = Array.Empty<string>();
        }
    }
}
