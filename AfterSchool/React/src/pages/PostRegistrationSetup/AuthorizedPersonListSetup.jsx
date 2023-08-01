import { Grid, Button, Container, Typography } from '@material-ui/core';
import React, { useEffect, useState } from 'react';
import { useFormik } from 'formik';
import * as Yup from 'yup';

import { phoneValidationRegex } from 'constants/regex';
import { usePostRegistration } from 'redux/hooks';
import AuthorizedPersonListRow from './AuthorizedPersonListRow';
import AddAuthorizedUserForm from './AddAuthorizedUserForm';
import { useParams } from 'react-router-dom';
import { useSnackbar } from 'notistack';
import useStyles from './styles';
import LoadingButton from 'components/Overrides/LoadingButton';
import { isEmpty } from 'lodash';

function AuthorizedPersonListSetup({ authorizedPersons, setActiveStep }) {
  const classes = useStyles();
  const { registrationUid } = useParams();
  const { enqueueSnackbar } = useSnackbar();
  const [initialValues, setInitialValue] = useState({});
  const [saveDisabled, setSaveDisabled] = useState(false);
  const [validationSchema, setValidationSchema] = useState({});
  const newUser = {
    ID: -1,
    FirstName: '',
    LastName: '',
    Phone: '',
    HasAccess: false,
    default: false,
    newObject: true,
    PersonPhoneDataType: 'NotSet'
  };

  const { msgs, updateData } = usePostRegistration();

  useEffect(() => {
    msgs.forEach(msg => enqueueSnackbar(msg.Text, { variant: 'error' }));
  }, [msgs]);

  useEffect(() => {
    const sessionCompleted = sessionStorage.getItem('isSessionCompleted');
    if (sessionCompleted) {
      setActiveStep(3);
    }
  }, []);

  useEffect(() => {
    const initialState = {};
    authorizedPersons.forEach(
      (person, index) =>
        (initialState[index] = { HasAccess: false, ...person, default: true, newObject: false })
    );
    setInitialValue(initialState);
  }, [authorizedPersons]);

  const formValidationSchema = () =>
    Yup.object().shape(
      Object.keys(values).reduce((acc, key) => {
        acc[key] = Yup.object().shape({
          Phone: Yup.string().when(['HasAccess'], {
            is: val => val,
            then: () =>
              Yup.string()
                .matches(phoneValidationRegex, 'Phone number is not valid')
                .required('Phone Number is required'),
            otherwise: () => Yup.string().notRequired()
          }),
          FirstName: Yup.string().when(['HasAccess'], {
            is: val => val,
            then: () => Yup.string().required('First Name is required'),
            otherwise: () => Yup.string()
          }),
          LastName: Yup.string().when(['HasAccess'], {
            is: val => val,
            then: () => Yup.string().required('Last Name is required'),
            otherwise: () => Yup.string()
          })
        });

        return acc;
      }, {})
    );

  const formik = useFormik({
    enableReinitialize: true,
    initialValues,
    validationSchema,
    onSubmit: async values => {
      setSaveDisabled(true);
      const filteredAuthorizedPersons = [];
      Object.keys(values).forEach(key => {
        const { ID, FirstName, LastName, Phone, HasAccess, PersonPhoneDataType } = values[key];
        if (ID !== -1 || (ID == -1 && HasAccess)) {
          const user = { ID, FirstName, LastName, Phone, HasAccess, PersonPhoneDataType };
          filteredAuthorizedPersons.push(user);
        }
      });
      await updateData({
        registrationUid,
        phone: '',
        pinCode: '',
        peopleList: filteredAuthorizedPersons
      }).then(apiErrors => {
        setSaveDisabled(false);
        if (apiErrors.length > 0) return;
        enqueueSnackbar('Submitted.', { variant: 'success' });
        sessionStorage.setItem('isSessionCompleted', true);
        setActiveStep(3);
      });
    }
  });

  const { setFieldValue, values, handleSubmit, errors, touched } = formik;

  useEffect(() => {
    setValidationSchema(formValidationSchema());
  }, [values]);

  const addNewUserHandler = () => {
    const key = Number(Object.keys(values).at(-1)) + 1;
    setFieldValue(key, newUser);
  };

  const handleDeleteField = fieldName => {
    const { [fieldName]: deletedField, ...newValues } = formik.values;
    formik.setValues(newValues);
    formik.setFieldTouched(fieldName, false);
    formik.setFieldError(fieldName, '');
  };

  const saveButtonText =
    !isEmpty(values) && Object.keys(values).some(key => values[key].HasAccess)
      ? 'Save & Notify'
      : 'Save';
  return (
    <Container maxWidth="md">
      <form onSubmit={handleSubmit}>
        <Grid container spacing={2} className={classes.alignLeft}>
          {Object.keys(values).map(key =>
            values[key].default ? (
              <AuthorizedPersonListRow
                person={values[key]}
                key={key}
                handleChange={() =>
                  setFieldValue(key, { ...values[key], HasAccess: !values[key].HasAccess })
                }
                handleEdit={() => setFieldValue(key, { ...values[key], default: false })}
                touched={touched[key]}
                errors={errors[key]}
              />
            ) : (
              <>
                <AddAuthorizedUserForm
                  person={values[key]}
                  setFieldValue={value => {
                    setFieldValue(key, value);
                  }}
                  handleDeleteField={() => handleDeleteField(key)}
                  id={key}
                />
              </>
            )
          )}

          <Grid item xs={12}>
            <Typography
              className={classes.addPersonButton}
              onClick={addNewUserHandler}
              data-testid="add-authorized-person-button"
            >
              Add another authorized person
            </Typography>
          </Grid>

          <Grid item xs={12} className={classes.listButtonWrapper}>
            <Button
              type="submit"
              variant="outlined"
              color="primary"
              fullWidth
              onClick={() => setActiveStep(3)}
              data-testid="cancel-button"
            >
              Cancel
            </Button>
            <LoadingButton
              type="submit"
              variant="contained"
              color="primary"
              fullWidth
              text={saveButtonText}
              loading={saveDisabled}
            />
          </Grid>
        </Grid>
      </form>
    </Container>
  );
}

export default AuthorizedPersonListSetup;
