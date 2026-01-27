namespace FormValidationTest.Client.Services.Validation;

public sealed class ValidationMessageLocalizer : IValidationMessageLocalizer
{
    private static readonly IReadOnlyDictionary<string, string> FinnishMessages =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["address_line1.length"] = "Osoiterivi 1 on liian pitkä.",
            ["address_line1.required"] = "Osoiterivi 1 on pakollinen.",
            ["address_line2.length"] = "Osoiterivi 2 on liian pitkä.",
            ["age.range"] = "Iän tulee olla välillä 18–120.",
            ["arr.range"] = "ARR-arvon tulee olla sallituissa rajoissa.",
            ["business_id.invalid"] = "Y-tunnus on virheellinen.",
            ["city.length"] = "Kaupunki on liian pitkä.",
            ["city.required"] = "Kaupunki on pakollinen.",
            ["contact_email.invalid"] = "Yhteyssähköposti on virheellinen.",
            ["contact_email.required"] = "Yhteyssähköposti on pakollinen.",
            ["contract_type.required"] = "Sopimustyyppi on pakollinen.",
            ["customer_name.length"] = "Asiakkaan nimi on liian pitkä.",
            ["customer_name.required"] = "Asiakkaan nimi on pakollinen.",
            ["email.invalid"] = "Sähköpostiosoite on virheellinen.",
            ["email.required"] = "Sähköpostiosoite on pakollinen.",
            ["industry.required"] = "Toimiala on pakollinen.",
            ["name.already_used"] = "Nimi on jo käytössä.",
            ["name.api_reserved"] = "Nimi ei voi olla 'ApiOnly'.",
            ["name.length"] = "Nimi on liian pitkä.",
            ["name.required"] = "Nimi on pakollinen.",
            ["name.server_reserved"] = "Nimi ei voi olla 'Server'.",
            ["notes.length"] = "Muistiinpanot ovat liian pitkät.",
            ["nullable_industry.required"] = "Toimiala on pakollinen.",
            ["optional_business_id.invalid"] = "Valinnainen Y-tunnus on virheellinen.",
            ["optional_decimal_fi.invalid"] = "Desimaaliluku on virheellinen.",
            ["optional_email.invalid"] = "Sähköposti on virheellinen.",
            ["optional_eur.invalid"] = "Euromäärä on virheellinen.",
            ["optional_eur.range"] = "Euromäärä on sallittujen rajojen ulkopuolella.",
            ["optional_finnish_ssn.invalid"] = "Henkilötunnus on virheellinen.",
            ["optional_iban.invalid"] = "IBAN on virheellinen.",
            ["optional_multi_choice.other_required"] = "Muu-vaihtoehto vaatii lisäarvon.",
            ["optional_percentage.invalid"] = "Prosenttiarvo on virheellinen.",
            ["optional_percentage.range"] = "Prosenttiarvo on sallittujen rajojen ulkopuolella.",
            ["optional_single_choice.other_required"] = "Muu-vaihtoehto vaatii lisäarvon.",
            ["phone.invalid"] = "Puhelinnumero on virheellinen.",
            ["phone.required"] = "Puhelinnumero on pakollinen.",
            ["postal_code.length"] = "Postinumero on liian pitkä.",
            ["postal_code.required"] = "Postinumero on pakollinen.",
            ["required_business_id.invalid"] = "Y-tunnus on virheellinen.",
            ["required_business_id.required"] = "Y-tunnus on pakollinen.",
            ["required_decimal_fi.invalid"] = "Desimaaliluku on virheellinen.",
            ["required_decimal_fi.required"] = "Desimaaliluku on pakollinen.",
            ["required_email.invalid"] = "Sähköposti on virheellinen.",
            ["required_email.required"] = "Sähköposti on pakollinen.",
            ["required_eur.invalid"] = "Euromäärä on virheellinen.",
            ["required_eur.range"] = "Euromäärä on sallittujen rajojen ulkopuolella.",
            ["required_eur.required"] = "Euromäärä on pakollinen.",
            ["required_finnish_ssn.invalid"] = "Henkilötunnus on virheellinen.",
            ["required_finnish_ssn.required"] = "Henkilötunnus on pakollinen.",
            ["required_iban.invalid"] = "IBAN on virheellinen.",
            ["required_iban.required"] = "IBAN on pakollinen.",
            ["required_multi_choice.other_required"] = "Muu-vaihtoehto vaatii lisäarvon.",
            ["required_multi_choice.required"] = "Valinta on pakollinen.",
            ["required_percentage.invalid"] = "Prosenttiarvo on virheellinen.",
            ["required_percentage.range"] = "Prosenttiarvo on sallittujen rajojen ulkopuolella.",
            ["required_percentage.required"] = "Prosenttiarvo on pakollinen.",
            ["required_single_choice.other_required"] = "Muu-vaihtoehto vaatii lisäarvon.",
            ["required_single_choice.required"] = "Valinta on pakollinen.",
            ["seats.range"] = "Paikkamäärä on sallittujen rajojen ulkopuolella.",
            ["sentinel_industry.required"] = "Toimiala on pakollinen.",
            ["ssn.invalid"] = "Henkilötunnus on virheellinen.",
            ["start_date.future"] = "Aloituspäivän tulee olla tulevaisuudessa.",
            ["vat_number.invalid"] = "ALV-numero on virheellinen.",
        };

    public string Localize(string errorCode, string? fallbackMessage = null)
    {
        if (string.IsNullOrWhiteSpace(errorCode))
        {
            return fallbackMessage ?? string.Empty;
        }

        if (FinnishMessages.TryGetValue(errorCode, out var localizedMessage))
        {
            return localizedMessage;
        }

        return fallbackMessage ?? errorCode;
    }

    public IReadOnlyList<string> LocalizeMany(
        IReadOnlyList<string> errorCodes,
        IReadOnlyList<string>? fallbackMessages = null
    )
    {
        if (errorCodes.Count == 0)
        {
            return Array.Empty<string>();
        }

        var localizedMessages = new List<string>(errorCodes.Count);
        for (var i = 0; i < errorCodes.Count; i++)
        {
            var fallbackMessage = ResolveFallbackMessage(fallbackMessages, i);
            localizedMessages.Add(Localize(errorCodes[i], fallbackMessage));
        }

        return localizedMessages;
    }

    private static string? ResolveFallbackMessage(IReadOnlyList<string>? fallbackMessages, int index)
    {
        if (fallbackMessages is null || fallbackMessages.Count == 0)
        {
            return null;
        }

        if (index < fallbackMessages.Count)
        {
            return fallbackMessages[index];
        }

        return fallbackMessages[0];
    }
}
