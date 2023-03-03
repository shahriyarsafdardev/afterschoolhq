import React, { useState, useEffect, useRef } from 'react';
import { Button, MenuItem } from '@material-ui/core';
import MenuPopover from 'components/MenuPopover/MenuPopover';
import useStyles from './styles';

function I18n() {
  const [open, setOpen] = useState(false);
  const [touched, setTouched] = useState(false);
  const [selectedLanguage, setSelectedLanguage] = useState('en');
  const [selectedLanguageName, setSelectedLanguageName] = useState('English');
  const menuRef = useRef(null);
  const classes = useStyles();

  const handleLanguageChange = event => {
    setSelectedLanguage(event.target.getAttribute('value'));
    setSelectedLanguageName(event.target.innerText);
    setOpen(false);
    setTouched(true);
  };

  function updateGoogleTranslateElement() {
    const element = document.getElementById('google_translate_element');
    if (element) {
      const select = element.querySelector('select');
      if (select && selectedLanguage) {
        // localStorage.setItem('languageSelected', selectedLanguage);
        select.value = selectedLanguage;
        select.dispatchEvent(new Event('change'));

        if (touched && selectedLanguage === 'en') window.location.reload(false);
      }
    }
  }

  const lang = {
    en: 'English',
    es: 'Español',
    fr: 'Français',
    ht: 'Kreyòl ayisyen',
    sw: 'Kiswahili'
  };
  //TODO: Language persistence between MVC and ReactApp is turned off for the time being
  //   useEffect(() => {
  //     const selectedLang = localStorage.getItem('languageSelected');
  //     if (selectedLang || selectedLang === 'null' || selectedLang === selectedLanguage) return;
  //     setSelectedLanguageName(lang[selectedLang]);
  //     setSelectedLanguage(selectedLang);
  //     updateGoogleTranslateElement();
  //   }, []);

  useEffect(() => {
    updateGoogleTranslateElement();
  }, [selectedLanguage, selectedLanguageName]);

  return (
    <div className="notranslate">
      <MenuPopover
        open={open}
        onClose={() => {
          setOpen(false);
        }}
        anchorEl={menuRef.current}
        className="notranslate"
      >
        <MenuItem value="en" onClick={handleLanguageChange}>
          English
        </MenuItem>
        <MenuItem value="es" onClick={handleLanguageChange}>
          Español
        </MenuItem>
        <MenuItem value="fr" onClick={handleLanguageChange}>
          Français
        </MenuItem>
        <MenuItem value="ht" onClick={handleLanguageChange}>
          Kreyòl ayisyen
        </MenuItem>
        <MenuItem value="sw" onClick={handleLanguageChange}>
          Kiswahili
        </MenuItem>
      </MenuPopover>
      <Button
        sx={{ display: { xs: 'block', md: 'none' } }}
        aria-label="more"
        aria-controls="long-menu"
        aria-haspopup="true"
        onClick={() => setOpen(!open)}
        ref={menuRef}
        className={classes.button}
      >
        {selectedLanguageName}
      </Button>
    </div>
  );
}

export default I18n;
