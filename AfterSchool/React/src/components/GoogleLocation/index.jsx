import React, { useRef, useEffect } from 'react';
import { TextField } from '@material-ui/core';
import { googleLocationAPI } from 'constants';

function GoogleLocation({ handlePlaceChanged, ...fieldProps }) {
  let autocomplete;
  const inputRef = useRef();

  useEffect(() => {
    const script = document.createElement('script');
    script.src = googleLocationAPI;
    script.async = true;
    const placeChangeHandler = () => {
      const selectedPlace = autocomplete.getPlace();
      handlePlaceChanged(selectedPlace);
    };
    document.head.appendChild(script);
    script.onload = () => {
      autocomplete = new window.google.maps.places.Autocomplete(inputRef.current);
      autocomplete.addListener('place_changed', placeChangeHandler);
    };

    return () => {
      window.removeEventListener('place_changed', placeChangeHandler);
      document.head.removeChild(script);
      window.google.maps = {};
    };
  }, []);

  return <TextField inputRef={inputRef} variant="outlined" fullWidth type="text" {...fieldProps} />;
}

export default GoogleLocation;
