import cs from 'classnames';
import useStyles from './styles';
import React, { useRef, useState, useEffect } from 'react';
import { StandaloneSearchBox, LoadScript } from '@react-google-maps/api';
import { Grid, Typography, TextField as MuiTextField } from '@material-ui/core';
import { locationObject } from 'components/SchoolField/SchoolField';

function ApiTextField({ formik, question, currentQuestions }) {
  const inputRef = useRef();
  const classes = useStyles();
  const [disabled, setDisable] = useState(true);
  const { touched, values, errors, setFieldValue } = formik;
  const { ID, Text, IsRequired: required = false } = question;
  const [typeClass, typeText] = required
    ? [classes.fieldLabelRequired, '*']
    : [classes.fieldLabelOptional, '(Optional)'];

  const setAddressFields = place => {
    currentQuestions.map(question => {
      const type = question.Text;
      switch (type) {
        case 'City':
          setFieldValue(question.ID, place.city || '');
          break;
        case 'Zip Code':
          setFieldValue(question.ID, place.postalcode || '');
          break;
        case 'State':
          if (place.state) {
            const options = question.Options.split(',').map(option => ({
              key: option,
              value: option
            }));
            const selectedOption = options.find(option => option.value === place.state);
            if (selectedOption) {
              setFieldValue(question.ID, JSON.stringify([selectedOption]));
            }
          }
          break;
      }
    });
  };

  const handlePlaceChanged = () => {
    const [place] = inputRef.current.getPlaces();
    if (place) setFieldValue(ID, place.name);
    setAddressFields(locationObject(place));
  };

  useEffect(() => {
    if (!values[11111] && disabled) return setDisable(false);
    setDisable(true);
    setFieldValue(ID, '');
  }, [values[11111]]);

  return (
    <Grid className={classes.textFieldBox} xs={12}>
      <Grid xs={12} md={4} item className={classes.titleGrid}>
        <Typography variant="h6" className={classes.fieldLabel}>
          {Text} &nbsp;
          <Typography className={typeClass}>{typeText}</Typography>
        </Typography>
      </Grid>
      <Grid xs={12} md={8} item className={classes.textFieldGrid}>
        <LoadScript googleMapsApiKey={process.env.GoogleMapsApiKey} libraries={['places']}>
          <StandaloneSearchBox
            onLoad={ref => {
              inputRef.current = ref;
            }}
            onPlacesChanged={handlePlaceChanged}
          >
            <MuiTextField
              variant="outlined"
              fullWidth
              {...formik.getFieldProps(question.ID)}
              error={touched[ID] && errors[ID]}
              helperText={touched[ID] && errors[ID]}
              type="text"
              className={cs('form-control', classes.formControlClass)}
              placeholder={`Enter your ${Text}`}
              disabled={disabled}
            />
          </StandaloneSearchBox>
        </LoadScript>
      </Grid>
    </Grid>
  );
}

export default ApiTextField;
