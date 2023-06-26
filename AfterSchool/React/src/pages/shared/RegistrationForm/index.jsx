import React from 'react';
import cs from 'classnames';
import { Form, FormikProvider } from 'formik';
import { Box, Button, Grid, Typography, CircularProgress } from '@material-ui/core';

import { Loader, ProgressBar, ShowQuestion } from 'components';
import { eSignMsg } from 'constants';
import useStyles from './styles';

export default function RegistrationForm(props) {
  const classes = useStyles();

  const {
    Name,
    isFree,
    formik,
    loading,
    skipAble,
    totalGroups,
    exitHandler,
    activeGroup,
    lastSection,
    handleSubmit,
    saveDisabled,
    firstSection,
    handleSection,
    registrantType,
    saveButtonText,
    allowToPayLater,
    showExit = true,
    isSurvey = false,
    currentQuestions,
    handleRedirection,
    isUpdating = false,
    isUpdatingRegistration = false
  } = props;
  const outerWrapperClass =
    isUpdating || isSurvey
      ? classes.outerWrapper
      : cs(classes.outerWrapper, classes.outerWrapperMarginTop);

  function eSignInfoText() {
    return (
      <Box className={classes.infoText}>
        <Typography>{eSignMsg(allowToPayLater)}</Typography>
      </Box>
    );
  }
  return (
    <FormikProvider value={formik}>
      <Form autoComplete="off" noValidate onSubmit={handleSubmit} className={classes.form}>
        {loading ? (
          <Loader />
        ) : (
          <Box className={outerWrapperClass}>
            {showExit &&
              (isUpdating ? (
                <Box className={classes.exitButtonWrapper}>
                  <Button
                    color="primary"
                    variant="outlined"
                    className={classes.backButton}
                    onClick={() => handleRedirection()}
                    data-testid={`Exit-${Name}`}
                  >
                    Exit
                  </Button>
                </Box>
              ) : (
                firstSection && (
                  <Box className={classes.exitButtonWrapper}>
                    <Button
                      color="primary"
                      variant="outlined"
                      className={classes.backButton}
                      onClick={exitHandler}
                      data-testid={`Exit-${Name}`}
                    >
                      Exit
                    </Button>
                  </Box>
                )
              ))}
            <ProgressBar activeGroup={activeGroup} totalGroups={totalGroups} />
            <Box className={classes.formWrapper}>
              <Box className={classes.formInnerWrapper}>
                <Typography className={classes.sectionName} variant="h6" color="primary">
                  {Name}
                </Typography>
                <Grid container className={cs(classes.justifyContentCenter, classes.leftPadding)}>
                  <Grid className={classes.innerContainer} item xs={12} md={12} lg={12}>
                    {currentQuestions?.map((question, index) =>
                      ShowQuestion({
                        index,
                        formik,
                        isUpdating,
                        isUpdatingRegistration,
                        currentQuestion: question,
                        currentQuestions: currentQuestions
                      })
                    )}
                  </Grid>
                </Grid>
                <Box className={classes.buttonOuterWrapper}>
                  {lastSection && !isUpdatingRegistration && !isFree && eSignInfoText()}
                  <Box className={classes.buttonWrapper}>
                    <Button
                      color="primary"
                      variant="outlined"
                      className={classes.backButton}
                      disabled={!activeGroup > 0}
                      onClick={() => handleSection('previous')}
                      data-testid={`Back-${Name}`}
                    >
                      Back
                    </Button>
                    <Button
                      data-testid={`Save-${Name}`}
                      color="primary"
                      className={classes.saveButton}
                      variant="contained"
                      type="submit"
                      disabled={saveDisabled}
                      key={activeGroup}
                    >
                      {saveDisabled ? (
                        <>
                          {'Loading . . .'} <CircularProgress size={'20px'} />
                        </>
                      ) : (
                        saveButtonText(lastSection, registrantType, isFree, skipAble)
                      )}
                    </Button>
                  </Box>
                </Box>
              </Box>
            </Box>
          </Box>
        )}
      </Form>
    </FormikProvider>
  );
}
