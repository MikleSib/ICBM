using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WorldMapStrategyKit {

    public class GameLogicExample : MonoBehaviour {

        public Dropdown countriesDropdown;
        public InputField resourcesInputField;
        public RectTransform infoPanel;
        public Text infoPanelCountryName, infoPanelCountryResources;

        const string RESOURCE_NAME = "Resources";
        const string ATTRIBUTES_FILENAME = "CountriesAttributes";


        WMSK map;
        List<string> countryNames;

        // Use this for initialization
        void Start() {

            map = WMSK.instance;

            PopulateCountryNames();
            LoadButtonClick();

            map.OnCountryClick += SelectCountryFromList;
            map.OnMouseMove += ShowTooltip;

        }

        void ShowTooltip(float x, float y) {

            Country country = map.GetCountry(new Vector2(x, y));
            if (country == null) {
                infoPanel.anchoredPosition = new Vector2(-300, 0);
                return;
            }
            infoPanelCountryName.text = country.name;
            infoPanelCountryResources.text = "$" + country.attrib["Resources"];
            infoPanel.anchoredPosition = map.input.mousePosition + new Vector3(10, -30);

        }

        void PopulateCountryNames() {
            countryNames = new List<string>();
            countryNames.Add("");
            countryNames.AddRange(map.GetCountryNames(false, false));

            countriesDropdown.ClearOptions();
            countriesDropdown.AddOptions(countryNames);
        }

        void SelectCountryFromList(int countryIndex, int regionIndex, int buttonIndex) {
            string countryName = map.GetCountry(countryIndex).name;
            countriesDropdown.value = countryNames.IndexOf(countryName);
        }


        public void CountrySelected(int index) {
            string countryName = countryNames[index];
            map.BlinkCountry(countryName, Color.red, Color.yellow, 2f, 0.3f);

            Country country = map.GetCountry(countryName);
            resourcesInputField.text = country.attrib[RESOURCE_NAME];
        }


        public void UpdateButtonClick() {
            string countryName = countriesDropdown.captionText.text;
            Country country = map.GetCountry(countryName);
            if (country == null)
                return;

            string resourcesValue = resourcesInputField.text;
            country.attrib[RESOURCE_NAME] = resourcesValue;

            GameObject txt = GameObject.Find(countryName + "_ResourcesText");
            if (txt == null) {
                TextMesh tm = map.AddMarker2DText("$" + resourcesValue, country.center);
                tm.transform.localScale *= 10f;
                tm.gameObject.name = countryName + "_ResourcesText";
            } else {
                txt.GetComponent<TextMesh>().text = "$" + resourcesValue;
            }
        }

        public void SaveButtonClick() {
            string countryAttributes = map.GetCountriesAttributes(true);
            System.IO.File.WriteAllText(ATTRIBUTES_FILENAME, countryAttributes, System.Text.Encoding.UTF8);
        }

        public void LoadButtonClick() {
            if (System.IO.File.Exists(ATTRIBUTES_FILENAME)) {
                string data = System.IO.File.ReadAllText(ATTRIBUTES_FILENAME);
                map.SetCountriesAttributes(data);
            }
        }



    }

}
