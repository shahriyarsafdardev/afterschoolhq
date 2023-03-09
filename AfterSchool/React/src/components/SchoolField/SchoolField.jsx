import cs from 'classnames';
import useStyles from './styles';
import SimpleCheckBox from 'components/SimpleCheckBox';
import { useRegistration } from 'redux/hooks';
import React, { useRef, useState, useEffect } from 'react';
import { StandaloneSearchBox, LoadScript } from '@react-google-maps/api';
import { Grid, Typography, TextField as MuiTextField } from '@material-ui/core';

export const locationObject = place => {
  const latitude = place?.geometry?.location?.lat();
  const longitude = place?.geometry?.location?.lng();
  const components = place.address_components;
  let {
    city,
    state,
    county,
    country,
    address1,
    address2,
    streetname,
    postalcode,
    streetnumber,
    neighborhood
  } = {};
  components.forEach(component => {
    const type = component.types[0];
    const longName = component.long_name;
    const shortName = component.short_name;

    switch (type) {
      case 'street_number':
        streetnumber = longName;
        address1 = longName;
        break;
      case 'route':
        streetname = shortName;
        break;
      case 'subpremise':
        address2 = longName;
        break;
      case 'neighborhood':
        neighborhood = longName;
        break;
      case 'postal_code':
        postalcode = longName;
        break;
      case 'locality':
        city = longName;
        break;
      case 'administrative_area_level_2':
        county = longName;
        break;
      case 'administrative_area_level_1':
        state = longName;
        break;
      case 'country':
        country = shortName;
        break;
      case 'sublocality_level_1':
        if (city.length === 0) city = longName;
        break;
    }
  });

  return {
    city,
    state,
    county,
    country,
    address1,
    address2,
    latitude,
    longitude,
    streetname,
    postalcode,
    streetnumber,
    neighborhood,
    locationName: place.name,
    googlePlaceId: place.place_id,
    formatted_address: place.formatted_address
  };
};

function SchoolField({ question }) {
  const inputRef = useRef();
  const classes = useStyles();
  const [schoolValue, setSchoolValue] = useState('');
  const { school, noSchool, setSchoolFieldValue } = useRegistration();
  const { Text, IsRequired: required = false } = question;
  const [typeClass, typeText] = required
    ? [classes.fieldLabelRequired, '*']
    : [classes.fieldLabelOptional, '(Optional)'];

  const handlePlaceChanged = () => {
    const [place] = inputRef.current.getPlaces();
    if (place) {
      setSchoolValue(place.name);
      setSchoolFieldValue({ school: locationObject(place), noSchool: false }, false);
    }
  };

  const changeHandler = event => setSchoolValue(event.target.value);

  useEffect(() => {
    setSchoolValue(school.locationName);
  }, [school?.name]);

  return (
    <>
      <Grid className={classes.textFieldBox} xs={12}>
        <Grid xs={12} md={4} item className={classes.titleGrid}>
          <Typography variant="h6" className={classes.fieldLabel}>
            {Text} &nbsp;
            <Typography className={typeClass}>{typeText}</Typography>
          </Typography>
        </Grid>
        <Grid xs={12} md={8} item className={classes.textFieldGrid}>
          <LoadScript
            googleMapsApiKey="AIzaSyBq47Ro2O69sQhVLEW4_2L0ClOBSW_Srmc"
            libraries={['places']}
          >
            <StandaloneSearchBox
              onLoad={ref => {
                inputRef.current = ref;
              }}
              onPlacesChanged={handlePlaceChanged}
            >
              <MuiTextField
                variant="outlined"
                fullWidth
                onChange={changeHandler}
                value={schoolValue}
                type="text"
                className={cs('form-control', classes.formControlClass)}
                placeholder={`Enter your ${Text}`}
                disabled={noSchool}
              />
            </StandaloneSearchBox>
          </LoadScript>
        </Grid>
      </Grid>
      {Text === 'School' && (
        <SimpleCheckBox
          question={{
            ...question,
            ID: 11111,
            Text: 'I can’t find my child’s school or my child is homeschooled'
          }}
          value={noSchool}
          setValue={value => {
            setSchoolFieldValue({ school: {}, noSchool: value }, false);
            setSchoolValue('');
          }}
        />
      )}
    </>
  );
}

export default SchoolField;
