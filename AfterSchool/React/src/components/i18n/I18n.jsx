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
    event.preventDefault();
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
        localStorage.setItem('languageSelected', selectedLanguage);
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

  const MenuItems = () =>
    Object.keys(lang).map(key => (
      <MenuItem key={key} value={key} onClick={handleLanguageChange} data-testid={lang[key]}>
        {lang[key]}
      </MenuItem>
    ));

  useEffect(() => {
    const selectedLang = localStorage.getItem('languageSelected');
    if (selectedLang && selectedLang !== 'null' && selectedLang !== selectedLanguage) {
      setSelectedLanguageName(lang[selectedLang]);
      setSelectedLanguage(selectedLang);
      updateGoogleTranslateElement();
    }
  }, []);

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
        <MenuItems />
      </MenuPopover>
      <Button
        sx={{ display: { xs: 'block', md: 'none' } }}
        aria-label="more"
        aria-controls="long-menu"
        aria-haspopup="true"
        onClick={() => setOpen(!open)}
        ref={menuRef}
        className={classes.button}
        data-testid={'i18n-button'}
      >
        {selectedLanguageName}
      </Button>
    </div>
  );
}

export default I18n;
