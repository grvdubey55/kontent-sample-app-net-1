﻿using System;
using System.Linq;
using DancingGoat.Models;
using Microsoft.Extensions.Options;
using DancingGoat.Areas.Admin.Abstractions;
using Microsoft.Extensions.Configuration;

namespace DancingGoat.Areas.Admin
{
    public class AppSettingProvider : IAppSettingProvider
    {
        private readonly AppSettings _settings;
        private readonly IConfiguration _configuration;


        private DateTime? _subscriptionExpiresAt;
        private Guid? _projectId;
        private Guid? _defaultProjectId;
        private string _previewApiKey;
        private string _kenticoKontentUrl;

        // Using IOptionsSnapshot to get the reloaded configuration on change
        public AppSettingProvider(IOptionsSnapshot<AppSettings> options, IConfiguration configuration)
        {
            _settings = options.Value;
            _configuration = configuration;
        }

        public DateTime? SubscriptionExpiresAt
        {
            get
            {
                if (_subscriptionExpiresAt.HasValue)
                {
                    return _subscriptionExpiresAt;
                }
                else
                {
                    if (DateTime.TryParse(_settings.SubscriptionExpiresAt, out DateTime subscriptionExpiresAt))
                    {
                        _subscriptionExpiresAt = subscriptionExpiresAt;

                        return _subscriptionExpiresAt;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            set
            {

                // TODO: Set configuration on the fly in asp.net core 2.x???

                if (value.HasValue)
                {
                    _settings.SubscriptionExpiresAt = value.Value.ToString("o");
                }

                // Creating new settings cannot be done through the indexer, hence .Add().
                /*if (ConfigurationManager.AppSettings.AllKeys.Contains(SUBSCRIPTION_EXPIRES_KEY_NAME))
            {
                _configuration.AppSettings.Settings[SUBSCRIPTION_EXPIRES_KEY_NAME].Value = value.Value.ToString("o");
            }
            else
            {
                _configuration.AppSettings.Settings.Add(SUBSCRIPTION_EXPIRES_KEY_NAME, value.Value.ToString("o"));
            }
            // Do we want to save to the appsettings.json those changes??
            //_configuration.Save();*/
                _subscriptionExpiresAt = value;
            }
        }

        public Guid? ProjectId
        {
            get
            {
                if (_projectId.HasValue)
                {
                    return _projectId;
                }
                else
                {
                    if (Guid.TryParse(_configuration.GetSection("DeliveryOptions").GetValue<string>("ProjectId"), out var projectId))
                    {
                        _projectId = projectId;

                        return _projectId;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            set
            {
                // TODO: same as the _subscriptionExpiresAt
                if (value.HasValue)
                {
                    _settings.ProjectId = value.Value.ToString();
                }

                _configuration["DeliveryOptions:ProjectId"] = value.Value.ToString();
                _projectId = value;
            }
        }

        public Guid? DefaultProjectId
        {
            get
            {
                if (_defaultProjectId.HasValue)
                {
                    return _defaultProjectId;
                }
                else
                {
                    if (Guid.TryParse(_settings.DefaultProjectId, out var projectId))
                    {
                        _defaultProjectId = projectId;

                        return _defaultProjectId;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        public string PreviewApiKey
        {
            get
            {
                if (!string.IsNullOrEmpty(_previewApiKey))
                {
                    return _previewApiKey;
                }
                else
                {
                    _previewApiKey = _settings.PreviewApiKey;
                    return _previewApiKey;
                }
            }
        }

        public string KenticoKontentUrl
        {
            get
            {
                if (!string.IsNullOrEmpty(_kenticoKontentUrl))
                {
                    return _kenticoKontentUrl;
                }
                else
                {
                    string url = null;

                    try
                    {
                        // TODO: Get from app settings
                        url = "https://app.kontent.ai/";
                    }
                    catch
                    {
                        // Return the default value below.
                    }

                    if (!string.IsNullOrEmpty(url))
                    {
                        _kenticoKontentUrl = url;

                        return _kenticoKontentUrl;
                    }
                    else
                    {
                        return @"https://app.kontent.ai";
                    }
                }
            }
        }

        public string[] ResponsiveWidths => _settings.ResponsiveWidths?.Split(',') ?? new string[]{};

        public bool ResponsiveImagesEnabled => ResponsiveWidths.Any();
        public Guid? GetProjectId()
        {
            return ProjectId;
        }

        public string GetKenticoKontentUrl()
        {
            return KenticoKontentUrl;
        }

        public Guid? GetDefaultProjectId()
        {
            return DefaultProjectId;
        }

        public void SetProjectId(Guid projectId)
        {
            ProjectId = projectId;
        }

        public void SetSubscriptionExpiresAt(DateTime? subscriptionExpiresAt)
        {
            SubscriptionExpiresAt = subscriptionExpiresAt;
        }
    }
}