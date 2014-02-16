﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows.Forms;
using TrackableEntities;

namespace TrackableItemTemplateWizard
{
    static class ModelTypesHelper
    {
        public static bool SetupUserInterface(List<Type> modelTypes,
            string dialogTitle, string dialogMessage, Form dialog,
            Label descLabel, ComboBox entityComboBox, ComboBox contextComboBox)
        {
            // Set dialog title and message
            dialog.Text = dialogTitle;
            descLabel.Text = dialogMessage;

            // Get trackable and context types
            List<TypeInfo> entityTypes = FilterModelTypes
                (modelTypes, typeof(ITrackable));

            // Populate entity combo
            if (entityTypes.Count == 0)
            {
                MessageBox.Show("Referenced projects do not contain any trackable entities." +
                    "\r\nAdd service entities using EF Power Tools then build the solution.",
                    "Trackable Entities Not Found");
                return false;
            }
            entityComboBox.DataSource = entityTypes;

            // Get dbContext types
            if (contextComboBox != null)
            {
                List<TypeInfo> contextTypes = FilterModelTypes
                        (modelTypes, typeof(DbContext));
                if (contextTypes.Count == 0)
                {
                    MessageBox.Show("Referenced projects do not contain any DbContext classes." +
                        "\r\nAdd service entities using EF Power Tools then build the solution.",
                        "DbContext Class Not Found");
                    return false;
                }
                contextComboBox.DataSource = contextTypes;
            }
            return true;
        }

        public static ModelTypesInfo GetModelTypesInfo(TextBox entitySetTextBox,
            ComboBox entityComboBox, ComboBox contextComboBox)
        {
            // Validate info
            if (!ValidateRequiredInfo(entitySetTextBox, entityComboBox,
                contextComboBox)) return null;

            // Get info
            string entityNamespace = ((Type) entityComboBox.SelectedValue).Namespace;
            string entityName = ((Type) entityComboBox.SelectedValue).Name;
            string entitySetName = entitySetTextBox.Text;
            string dbContextName = null;
            if (contextComboBox != null)
            {
                dbContextName = ((Type) contextComboBox.SelectedValue).Name;
            }
            var info = new ModelTypesInfo
            {
                EntityNamespace = entityNamespace,
                EntityName = entityName,
                EntitySetName = entitySetName,
                DbContextName = dbContextName
            };
            return info;
        }

        private static bool ValidateRequiredInfo(TextBox entitySetTextBox,
            ComboBox entityComboBox, ComboBox contextComboBox)
        {
            if (entityComboBox.SelectedIndex == -1)
            {
                MessageBox.Show("Select an Entity Name", "Entity Name");
                return false;
            }

            if (string.IsNullOrWhiteSpace(entitySetTextBox.Text))
            {
                MessageBox.Show("Enter an Entity Set Name", "Entity Set Name");
                return false;
            }

            if (contextComboBox != null && contextComboBox.SelectedIndex == -1)
            {
                MessageBox.Show("Select an DbContext Name", "DbContext Name");
                return false;
            }
            return true;
        }

        private static List<TypeInfo> FilterModelTypes
            (IEnumerable<Type> modelTypes, Type canAssignTo)
        {
            var trackableTypes = from t in modelTypes
                where canAssignTo.IsAssignableFrom(t)
                select new TypeInfo
                {
                    DisplayName = string.Format("{0} ({1})",
                        t.Name, t.Namespace),
                    Type = t
                };
            return trackableTypes.ToList();
        }
    }
}